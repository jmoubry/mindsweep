using Microsoft.Phone.Shell;
using Mindsweep.Helpers;
using Mindsweep.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            IsSynced = LastSync > DateTime.UtcNow.AddMinutes(-15);
        }

        #region Public Members

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

        // Active projects.
        private ObservableCollection<Project> _activeProjects;
        public ObservableCollection<Project> ActiveProjects
        {
            get { return _activeProjects; }
            set
            {
                _activeProjects = value;
                NotifyPropertyChanged("ActiveProjects");
            }
        }


        // All task series.
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

        // All tasks.
        private ObservableCollection<Task> _allTasks;
        public ObservableCollection<Task> AllTasks
        {
            get { return _allTasks; }
            set
            {
                _allTasks = value;
                NotifyPropertyChanged("AllTasks");
            }
        }

        // All overdue tasks.
        private ObservableCollection<Task> _allOverdueTasks;
        public ObservableCollection<Task> AllOverdueTasks
        {
            get { return _allOverdueTasks; }
            set
            {
                _allOverdueTasks = value;
                NotifyPropertyChanged("AllOverdueTasks");
            }
        }

        // All due today tasks.
        private ObservableCollection<Task> _tasksDueToday;
        public ObservableCollection<Task> TasksDueToday
        {
            get { return _tasksDueToday; }
            set
            {
                _tasksDueToday = value;
                NotifyPropertyChanged("TasksDueToday");
            }
        }

        // All due tomorrow tasks.
        private ObservableCollection<Task> _tasksDueTomorrow;
        public ObservableCollection<Task> TasksDueTomorrow
        {
            get { return _tasksDueTomorrow; }
            set
            {
                _tasksDueTomorrow = value;
                NotifyPropertyChanged("TasksDueTomorrow");
            }
        }

        // All due this week tasks.
        private ObservableCollection<Task> _tasksDueThisWeek;
        public ObservableCollection<Task> TasksDueThisWeek
        {
            get { return _tasksDueThisWeek; }
            set
            {
                _tasksDueThisWeek = value;
                NotifyPropertyChanged("TasksDueThisWeek");
            }
        }

        // All someday tasks.
        private ObservableCollection<Task> _tasksDueSomeday;
        public ObservableCollection<Task> TasksDueSomeday
        {
            get { return _tasksDueSomeday; }
            set
            {
                _tasksDueSomeday = value;
                NotifyPropertyChanged("TasksDueSomeday");
            }
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
                NotifyPropertyChanged("InboxOpenTaskCount");
                NotifyPropertyChanged("NextActionsCount");
            }
        }

        public int InboxOpenTaskCount
        {
            get 
            {
                var inbox = AllProjects.Where(Exp.IsInbox).FirstOrDefault();

                if (inbox == null)
                    return 0;

                return AllTasks.Where(t => t.TaskSeries.Project.Id == inbox.Id).Count(Exp.IsOpen);
            }
        }

        public int NextActionsCount
        {
            get
            {
                var inbox = AllProjects.Where(Exp.IsInbox).FirstOrDefault();

                if (inbox == null)
                    return 0;

                return AllTasks.Count(Exp.IsNextAction);
            }
        }

        #endregion

        // Write changes in the data context to the database.
        public void SaveChangesToDB()
        {
            mainDB.SubmitChanges();
        }

        // Query database and load the collections and list used by the pivot pages.
        public void LoadCollectionsFromDatabase()
        {
            AllProjects = new ObservableCollection<Project>(mainDB.Projects.OrderBy(p => p.Position));
            ActiveProjects = new ObservableCollection<Project>(mainDB.Projects.Where(Exp.IsActive).OrderBy(p => p.Position));
            AllTaskSeries = new ObservableCollection<TaskSeries>(mainDB.TaskSeries);
            AllTasks = new ObservableCollection<Task>(mainDB.Tasks);

            AllOverdueTasks = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsOverdue).OrderBy(t=>t.Due).ThenBy(t=> t.Priority).ThenBy(t => t.TaskSeries.Name));
            
            TasksDueToday = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsDueToday).OrderBy(t => t.Due).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));
            TasksDueTomorrow = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsDueTomorrow).OrderBy(t => t.Due).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));
            TasksDueThisWeek = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsDueThisWeek).OrderBy(t => t.Due).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));

            TasksDueSomeday = new ObservableCollection<Task>(mainDB.Tasks.Where(t => !t.Due.HasValue).OrderBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));

            UpdateTile(AllOverdueTasks.Count + TasksDueToday.Count);
        }

        public void UpdateTile(int count)
        {
            var mainTile = ShellTile.ActiveTiles.First();
            mainTile.Update(new StandardTileData
            {
                Count = count
            });
        }

        WebClient client;

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // TODO: handle error

            (e.UserState as Action<string>)(e.Result);   
        }

        public class UserInfo
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
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
            var bw = new BackgroundWorker();

            bw.DoWork += (s, args) =>
            {
                // This runs on a background thread.

                JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(json);

                if (response.Status == StatusCodes.InvalidAuthToken)
                    Logout();

                foreach (Project serverProject in response.Projects)
                {
                    var localProject = mainDB.Projects.Where(p => p.Id == serverProject.Id).FirstOrDefault();

                    if (localProject == null)
                    {
                        AddProject(serverProject);
                    }
                    else
                    {
                        _SyncProject(localProject, serverProject);
                    }
                }

                // TODO: delete projects that don't exist on the server.

            };

            bw.RunWorkerCompleted += (s, args) =>
            {
                // Do your UI work here this will run on the UI thread.
                // Clear progress bar.


                mainDB.SubmitChanges();

                AllProjects = new ObservableCollection<Project>(mainDB.Projects);

                // If we haven't downloaded tasks for this user, just grab the incomplete ones.
                if (mainDB.TaskSeries.Count() == 0 || LastSync == DateTime.MinValue)
                    client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETTASKS + "&filter=status:incomplete"), new Action<string>(ParseJsonTasks));
                else // Download the changes since the last sync.
                    client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETTASKS + "&last_sync=" + LastSync.ToString("o")), new Action<string>(ParseJsonTasks));
            };

            // Set progress bar.


            bw.RunWorkerAsync();
        }

        public event EventHandler SyncCompleted;

        protected virtual void OnSyncCompleted(EventArgs e)
        {
            if (SyncCompleted != null)
                SyncCompleted(this, e);
        }

        public void ParseJsonTasks(string json)
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (s, args) =>
            {
                // This runs on a background thread.

                JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(json);

                if (response.Status == StatusCodes.InvalidAuthToken)
                    Logout();

                _UpdateTasks(response.TasksByProject);
            };
            bw.RunWorkerCompleted += (s, args) =>
            {
                mainDB.SubmitChanges();

                IsSynced = true;
                LastSync = DateTime.UtcNow;

                LoadCollectionsFromDatabase();

                OnSyncCompleted(EventArgs.Empty);

                // Do your UI work here this will run on the UI thread.
                // Clear progress bar.
            };

            // Set progress bar.

            bw.RunWorkerAsync();
        }

        private void _UpdateTasks(List<Project> projects)
        {
            foreach (Project serverProject in projects)
            {
                var localProject = mainDB.Projects.Where(p => p.Id == serverProject.Id).FirstOrDefault();

                if (localProject != null)
                {
                    foreach (TaskSeries serverTaskSeries in serverProject.TaskSeries)
                    {
                        var localTaskSeries = mainDB.TaskSeries.Where(t => t.Id == serverTaskSeries.Id).FirstOrDefault();

                        if (localTaskSeries == null)
                        {
                            serverTaskSeries.Project = localProject;

                            if (serverTaskSeries.RepeatRule != null)
                                serverTaskSeries.RepeatRule = mainDB.RepeatRules.Where(rr => rr.Rule == serverTaskSeries.RepeatRule.Rule).FirstOrDefault() ?? serverTaskSeries.RepeatRule;

                            AddTaskSeries(serverTaskSeries);
                        }
                        else
                        {
                            _SyncTaskSeries(localProject, localTaskSeries, serverTaskSeries);
                        }
                    }
                }
            }
        }

        private void _SyncProject(Project localProject, Project serverProject)
        {
            localProject.Name = serverProject.Name;
            localProject.Deleted = serverProject.Deleted;
            localProject.Locked = serverProject.Locked;
            localProject.Archived = serverProject.Archived;
            localProject.Smart = serverProject.Smart;
            localProject.Position = serverProject.Position;
        }

        private void _SyncTaskSeries(Project localProject, TaskSeries localTaskSeries, TaskSeries serverTaskSeries)
        {
            // Update if newer.
            if (localTaskSeries.Modified < serverTaskSeries.Modified)
            {
                localTaskSeries.Created = serverTaskSeries.Created;
                localTaskSeries.Modified = serverTaskSeries.Modified;
                localTaskSeries.Name = serverTaskSeries.Name;
                localTaskSeries.Project = localProject;

                if (serverTaskSeries.RepeatRule != null)
                    localTaskSeries.RepeatRule = mainDB.RepeatRules.Where(rr => rr.Rule == serverTaskSeries.RepeatRule.Rule).FirstOrDefault() ?? serverTaskSeries.RepeatRule;

                localTaskSeries.Source = serverTaskSeries.Source;
                localTaskSeries.Tags = serverTaskSeries.Tags;
                localTaskSeries.Url = serverTaskSeries.Url;

                foreach (Task serverTask in serverTaskSeries.Tasks)
                {
                    var localTask = localTaskSeries.Tasks.Where(tt => tt.Id == serverTask.Id).FirstOrDefault();

                    if (localTask == null)
                        localTaskSeries.Tasks.Add(serverTask);
                    else
                        _SyncTask(localTask, serverTask);
                }

                // Look for any tasks that need to be removed.
                foreach (Task localTaskToRemove in localTaskSeries.Tasks.Except(serverTaskSeries.Tasks, new TaskComparer()).ToList())
                    localTaskSeries.Tasks.Remove(localTaskToRemove);
            }
        }

        private void _SyncTask(Task localTask, Task serverTask)
        {
            localTask.Due = serverTask.Due;
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
                    page.NavigationService.Navigate(new Uri("/Views/MainPage.xaml", UriKind.RelativeOrAbsolute));
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
