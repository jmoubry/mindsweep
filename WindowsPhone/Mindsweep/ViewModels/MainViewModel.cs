using Mindsweep.Helpers;
using Mindsweep.Model;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Mindsweep.ViewModels
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh286405(v=vs.105).aspx
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // LINQ to SQL data context for the local database.
        private MainDataContext mainDB;

        // Class constructor, create the data context object.
        public MainViewModel(string deckDBConnectionString)
        {
            mainDB = new MainDataContext(deckDBConnectionString);
            client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            IsSynced = false;
        }

        // All projects.
        private ObservableCollection<Project> _allProjects;
        public ObservableCollection<Project> AllProjects
        {
            get { return _allProjects; }
            set
            {
                _allProjects = value;
                NotifyPropertyChanged("AllProjects");
            }
        }

        // All tasks.
        private ObservableCollection<TaskSeries> _allTaskSeries;
        public ObservableCollection<TaskSeries> AllTaskSeries
        {
            get { return _allTaskSeries; }
            set
            {
                _allTaskSeries = value;
                NotifyPropertyChanged("AllTaskSeries");
            }
        }

        // Write changes in the data context to the database.
        public void SaveChangesToDB()
        {
            mainDB.SubmitChanges();
        }

        private bool _isAuthorized;
        public bool IsAuthorized
        {
            get { return _isAuthorized; }
            set
            {
                _isAuthorized = value;
                NotifyPropertyChanged("IsAuthorized");
            }
        }

        // Query database and load the collections and list used by the pivot pages.
        public void LoadCollectionsFromDatabase()
        {
            AllProjects = new ObservableCollection<Project>(mainDB.Projects);
            AllTaskSeries = new ObservableCollection<TaskSeries>(mainDB.TaskSeries);
        }

        WebClient client;


        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // TODO: handle error
            (e.UserState as Action<string>)(e.Result);
        }

        public bool IsLoggedIn
        {
            get { return !string.IsNullOrEmpty(this.Token); }
        }

        public bool IsSynced { get; set; }

        private string _token;
        public string Token
        {
            get
            {
                if (string.IsNullOrEmpty(_token))
                    IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("Token", out _token);

                return _token;
            }
            set
            {
                _token = value;
                IsolatedStorageSettings.ApplicationSettings["Token"] = _token;
                NotifyPropertyChanged("Token");
                NotifyPropertyChanged("IsLoggedIn");
            }
        }

        public class UserInfo
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
        }

        private UserInfo _user;
        public UserInfo User
        {
            get
            {
                if (_user == null)
                   IsolatedStorageSettings.ApplicationSettings.TryGetValue<UserInfo>("User", out _user);

                return _user;
            }
            set
            {
                _user = value;
                IsolatedStorageSettings.ApplicationSettings["User"] = _user;
                NotifyPropertyChanged("User");
            }
        }

        public void Login(string token)
        {
            this.Token = token;
        }
        public void Logout()
        {
            this.Token = null;
        }

        public void ParseJsonProjects(string json)
        {
            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(json);

            if (response.Status == StatusCodes.InvalidAuthToken)
                Logout();

            foreach (Project proj in response.Projects)
            {
                var storedProj = mainDB.Projects.Where(p => p.Id == proj.Id).FirstOrDefault();

                if (storedProj == null)
                {
                    AddProject(proj);
                }
                else
                {
                    storedProj.Sync(proj);
                }
            }

            // TODO: delete projects that don't exist on the server.

            mainDB.SubmitChanges();

            AllProjects = new ObservableCollection<Project>(mainDB.Projects);

            // If we haven't downloaded tasks for this user, just grab the incomplete ones.
            if(mainDB.TaskSeries.Count() == 0)
                client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETTASKS + "&filter=status:incomplete"), new Action<string>(ParseJsonTasks));
            else // Download the changes since the last sync.
                client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETTASKS + "&last_sync=" + LastSync.ToString("o")), new Action<string>(ParseJsonTasks));
        }

        private DateTime _lastSync;
        public DateTime LastSync
        {
            get
            {
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>("LastSync", out _lastSync))
                    _lastSync = DateTime.MinValue;

                return _lastSync;
            }
            set
            {
                _lastSync = value;
                IsolatedStorageSettings.ApplicationSettings["LastSync"] = _lastSync;
                NotifyPropertyChanged("LastSync");
            }
        }

        public void ParseJsonTasks(string json)
        {
            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(json);

            if (response.Status == StatusCodes.InvalidAuthToken)
                Logout();

            foreach (Project proj in response.TasksByProject)
            {
                var storedProject = mainDB.Projects.Where(p => p.Id == proj.Id).FirstOrDefault();

                if (storedProject != null)
                {
                    foreach (TaskSeries ts in proj.TaskSeries)
                    {
                        var storedTaskSeries = mainDB.TaskSeries.Where(t => t.Id == ts.Id).FirstOrDefault();

                        if (storedTaskSeries == null)
                        {
                            ts.Project = storedProject;
                            AddTaskSeries(ts);
                        }
                        else
                        {
                            storedTaskSeries.Sync(ts);
                        }
                    }
                }
            }

            mainDB.SubmitChanges();

            IsSynced = true;
            LastSync = DateTime.UtcNow;

            LoadCollectionsFromDatabase();
        }

        public void Sync()
        {
            client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETLISTS), new Action<string>(ParseJsonProjects));

            // TODO: Sync the other way -- send updates to RTM.
        }

        public void DeleteDB()
        {
            mainDB.DeleteDatabase();
            LoadCollectionsFromDatabase();
        }

        // Add a project to the database and collections.
        public void AddProject(Project newProject)
        {
            // Add a project to the data context.
            mainDB.Projects.InsertOnSubmit(newProject);

            // Save changes to the database.
            mainDB.SubmitChanges();

            // Add a project to the "all" observable collection.
            AllProjects.Add(newProject);
        }

        // Add a project to the database and collections.
        public void AddTaskSeries(TaskSeries newTaskSeries)
        {
            // Add a project to the data context.
            mainDB.TaskSeries.InsertOnSubmit(newTaskSeries);

            // Save changes to the database.
            mainDB.SubmitChanges();

            // Add a project to the "all" observable collection.
            AllTaskSeries.Add(newTaskSeries);
        }

        public void ConfirmAndDeleteProject(Page page, Project project)
        {
            RadMessageBox.Show("Confirm delete", buttons: MessageBoxButtons.YesNo, message: "Are you sure you want to delete the \"" + project.Name + "\" project?", closedHandler: (args) =>
            {
                if (args.Result == DialogResult.OK)
                {
                    FlurryWP7SDK.Api.LogEvent("Delete Project");

                    this.DeleteProject(project);
                    page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                }
            });
        }

        // Remove a project from the database and collections.
        public void DeleteProject(Project projectForDelete)
        {
            // Remove the deck from the "all" observable collection.
            AllProjects.Remove(projectForDelete);

            // Remove the deck from the data context.
            mainDB.Projects.DeleteOnSubmit(projectForDelete);

            foreach (TaskSeries taskseries in projectForDelete.TaskSeries)
            {
                AllTaskSeries.Remove(taskseries);
                mainDB.TaskSeries.DeleteOnSubmit(taskseries);
            }

            // Save changes to the database.
            mainDB.SubmitChanges();
        }
        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
