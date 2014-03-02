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
using System.Windows;
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
            clientForUpdates = new WebClient();
            clientForUpdates.DownloadStringCompleted += clientForUpdates_DownloadStringCompleted;
            clientForTimeline = new WebClient();
            clientForTimeline.DownloadStringCompleted += clientForTimeline_DownloadStringCompleted;
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

                return AllTasks.Where(t => t.TaskSeries != null && t.TaskSeries.Project != null && t.TaskSeries.Project.Id == inbox.Id).Count(Exp.IsOpen);
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

            AllTasks = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsOpen));

            AllOverdueTasks = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsOverdue).OrderBy(t=>t.DueInLocalTime).ThenBy(t=> t.Priority).ThenBy(t => t.TaskSeries.Name));

            TasksDueToday = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsDueToday).OrderBy(t => t.DueInLocalTime).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));
            TasksDueTomorrow = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsDueTomorrow).OrderBy(t => t.DueInLocalTime).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));
            TasksDueThisWeek = new ObservableCollection<Task>(mainDB.Tasks.Where(Exp.IsDueThisWeek).OrderBy(t => t.DueInLocalTime).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));

            TasksDueSomeday = new ObservableCollection<Task>(mainDB.Tasks.Where(t => !t.Due.HasValue).OrderBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name));

            NotifyPropertyChanged("AllProjects");
            NotifyPropertyChanged("ActiveProjects");
            NotifyPropertyChanged("AllTasks");
            NotifyPropertyChanged("AllOverdueTasks");
            NotifyPropertyChanged("TasksDueToday");
            NotifyPropertyChanged("TasksDueTomorrow");
            NotifyPropertyChanged("TasksDueThisWeek");
            NotifyPropertyChanged("TasksDueSomeday");
            NotifyPropertyChanged("NextActionsCount");
            NotifyPropertyChanged("InboxOpenTaskCount");

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
        WebClient clientForUpdates;
        WebClient clientForTimeline;

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // TODO: handle error
            if (e.Error != null) // TODO: && Error = No Internet
            {
            }
            else
                (e.UserState as Action<string>)(e.Result);
        }

        void clientForTimeline_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // TODO: handle error
            if (e.Error != null) // TODO: && Error = No Internet
            {
            }
            else
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
            this.LastSync = DateTime.MinValue;
            this.IsSynced = false;
            this.WipeDB();
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
                    // See if project is stored locally.
                    var localProject = mainDB.Projects.Where(p => p.Id == serverProject.Id).FirstOrDefault();

                    if (localProject == null && !serverProject.Deleted)
                    {
                        // Project exists on the server, but not locally.
                        AddProject(serverProject);
                    }
                    else if (localProject != null && serverProject.Deleted)
                    {
                        // Project exists locally, but is marked deleted on the server.
                        mainDB.Projects.DeleteOnSubmit(localProject);
                    }
                    else if (localProject != null)
                    {
                        // Project exists both locally and on the server
                        _SyncProject(localProject, serverProject);
                    } 
                }

                // Loop through projects that exist locally, but are not on the server
                foreach (Project toDelete in mainDB.Projects.ToList().Except(response.Projects, new ProjectComparer()).ToList())
                {
                    // Cheap way to avoid null task references...don't delete the project from the db if it has tasks.
                    // The tasks will have been moved to Inbox, so the next time the sync occurs, the project will have 0 tasks and will be deleted from db.
                    if (toDelete.TaskSeries.Count == 0)
                        mainDB.Projects.DeleteOnSubmit(toDelete);
                    else
                        toDelete.Deleted = true;
                }

                mainDB.SubmitChanges();
            };

            bw.RunWorkerCompleted += (s, args) =>
            {
                // Do your UI work here this will run on the UI thread.
                AllProjects = new ObservableCollection<Project>(mainDB.Projects);

                // If we haven't downloaded tasks for this user, just grab the incomplete ones.
                if (mainDB.TaskSeries.Count() == 0 || LastSync == DateTime.MinValue)
                    client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETTASKS + "&filter=status:incomplete"), new Action<string>(ParseJsonTasks));
                else // Download the changes since the last sync.
                    client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETTASKS + "&last_sync=" + LastSync.ToString("o")), new Action<string>(ParseJsonTasks));
            };

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

                _UpdateTasks(response);
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

                ProcessRequestQueue();
            };

            // Set progress bar.

            bw.RunWorkerAsync();
        }

        private void _UpdateTasks(JsonResponse response)
        {
            foreach (Project serverProject in response.TasksByProject)
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

            // Handle deleted tasks.
            foreach (Project serverProject in response.DeletedTasksByProject)
            {
                var localProject = mainDB.Projects.Where(p => p.Id == serverProject.Id).FirstOrDefault();
                if (localProject != null)
                {
                    foreach (TaskSeries serverTaskSeries in serverProject.TaskSeries)
                    {
                        foreach (Task serverDeletedTask in serverTaskSeries.Tasks)
                        {
                            // Mark task deleted.
                            // Note: tried deleting task from db but it caused other tasks in the series to have null references.
                            var localTaskToDelete = mainDB.Tasks.Where(t => t.Id == serverDeletedTask.Id && t.TaskSeries.Id == serverTaskSeries.Id).FirstOrDefault();
                            if (localTaskToDelete != null)
                                localTaskToDelete.Deleted = DateTime.UtcNow;
                        }
                    }
                }
            }

            // Save changes to the database.
            mainDB.SubmitChanges();
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
            }
        }

        private void _SyncTask(Task localTask, Task serverTask)
        {
            localTask.Added = serverTask.Added;
            localTask.Completed = serverTask.Completed;
            localTask.Deleted = serverTask.Deleted;
            localTask.Due = serverTask.Due;
            localTask.Estimate = serverTask.Estimate;
            localTask.HasDueTime = serverTask.HasDueTime;
            localTask.Postponed = serverTask.Postponed;
            localTask.Priority = serverTask.Priority;                        
        }

        public void Sync()
        {
            client.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GETLISTS), new Action<string>(ParseJsonProjects));
        }

        public void DeleteDB()
        {
            mainDB.DeleteDatabase();
        }

        public void WipeDB()
        {
            mainDB.RequestQueue.DeleteAllOnSubmit(mainDB.RequestQueue);
            mainDB.Tasks.DeleteAllOnSubmit(mainDB.Tasks);
            mainDB.TaskSeries.DeleteAllOnSubmit(mainDB.TaskSeries);
            mainDB.Projects.DeleteAllOnSubmit(mainDB.Projects);
            mainDB.RepeatRules.DeleteAllOnSubmit(mainDB.RepeatRules);

            mainDB.SubmitChanges();
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

        // Add a task series to the database and collections.
        public void AddTaskSeries(TaskSeries newTaskSeries)
        {
            // Add a project to the data context.
            mainDB.TaskSeries.InsertOnSubmit(newTaskSeries);

            // Save changes to the database.
            mainDB.SubmitChanges();

            // Add a project to the "all" observable collection.
            newTaskSeries.Tasks.ToList().ForEach(t => AllTasks.Add(t));
        }

        #region Code for Updates

        public string Timeline;

        public void QueueRequest(string requestUri, string localTaskSeriesIdForAdd = null)
        {
            // Add a project to the data context.
            mainDB.RequestQueue.InsertOnSubmit(new Request { LocalTaskSeriesIdForAdd = localTaskSeriesIdForAdd, RequestUri = requestUri, Requested = DateTime.UtcNow });

            // Save changes to the database.
            mainDB.SubmitChanges();

            ProcessRequestQueue();
        }

        private void RequestTimeline()
        {
            if (string.IsNullOrEmpty(Timeline))
                clientForTimeline.DownloadStringAsync(RTM.SignJsonRequest(RTM.URI_GET_TIMELINE), new Action<string>(SetTimeline));
        }

        private bool _isProcessingRequests = false;

        public void ProcessRequestQueue()
        {
            if (_isProcessingRequests)
                return;

            if (string.IsNullOrEmpty(Timeline))
            {
                RequestTimeline();
                return;
            }

            _isProcessingRequests = true;

            var nextRequest = mainDB.RequestQueue.OrderBy(r => r.Requested).Skip(skipRequests).FirstOrDefault();

            if (nextRequest != null)
                clientForUpdates.DownloadStringAsync(RTM.SignJsonRequest(nextRequest.RequestUri + "&timeline=" + Timeline), nextRequest);
            else
                _isProcessingRequests = false;
        }

        void clientForUpdates_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null) // and is related to no internet
            {
                _isProcessingRequests = false;
                return;
            }

            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(e.Result);

            if (response.Status == StatusCodes.InvalidAuthToken)
            {
                _isProcessingRequests = false;
                Logout();
            }

            if (response.Status == StatusCodes.Success)
            {
                mainDB.RequestQueue.DeleteOnSubmit(e.UserState as Request);
                mainDB.SubmitChanges();
            }
            else
            {
                // Skip this request and try other ones.
                skipRequests++;
            }

            _isProcessingRequests = false;

            ProcessRequestQueue();
        }

        private int skipRequests = 0;

        public void SetTimeline(string json)
        {
            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(json);

            if (response.Status == StatusCodes.InvalidAuthToken)
                Logout();

            this.Timeline = response.rsp.timeline;

            ProcessRequestQueue();
        }

        public void Add(Task task)
        {
            task.Id = Guid.NewGuid().ToString();
            task.Added = DateTime.UtcNow;
            task.TaskSeries.Id = Guid.NewGuid().ToString();
            task.TaskSeries.Created = DateTime.UtcNow;
            task.TaskSeries.Modified = DateTime.UtcNow;
            task.TaskSeries.Project = AllProjects.Where(Exp.IsInbox).FirstOrDefault();
            task.TaskSeries.Source = "Mindsweep";

            mainDB.Tasks.InsertOnSubmit(task);
            mainDB.SubmitChanges();

            LoadCollectionsFromDatabase();

            QueueRequest(string.Format("{0}&list_id={1}&name={2}&parse={3}", RTM.URI_ADDTASK, task.TaskSeries.Project.Id, task.TaskSeries.Name, "0"), task.TaskSeries.Id);
        }

        // Mark task completed.
        public void Complete(Task task)
        {
            task.Completed = DateTime.UtcNow;
            task.TaskSeries.Modified = DateTime.UtcNow;

            // TODO: Handle repeat logic.

            // Save changes to the database.
            mainDB.SubmitChanges();

            LoadCollectionsFromDatabase();
            
            QueueRequest(string.Format("{0}&list_id={1}&taskseries_id={2}&task_id={3}", RTM.URI_SETCOMPLETE, task.TaskSeries.Project.Id, task.TaskSeries.Id, task.Id));
        }

        // Postpone task.
        public void Postpone(Task task)
        {
            task.Postponed += 1;
            task.Due = task.Due.HasValue ? task.Due.Value.AddDays(1) : DateTime.UtcNow;
            task.TaskSeries.Modified = DateTime.UtcNow;

            // Save changes to the database.
            mainDB.SubmitChanges();

            LoadCollectionsFromDatabase();

            QueueRequest(string.Format("{0}&list_id={1}&taskseries_id={2}&task_id={3}", RTM.URI_POSTPONE, task.TaskSeries.Project.Id, task.TaskSeries.Id, task.Id));
        }

        public void ConfirmAndDelete(List<Task> tasks, Page page = null, bool navigateBack = false)
        {
            string message = string.Format("Are you sure you want to delete the selected {0} tasks?", tasks.Count);

            if (tasks.Count == 1)
                message = "Are you sure you want to delete the task, \"" + tasks.First().TaskSeries.Name + "\"?";

            RadMessageBox.Show("Confirm delete", buttons: MessageBoxButtons.YesNo, message: message, closedHandler: (args) =>
            {
                if (args.Result == DialogResult.OK)
                {
                    FlurryWP7SDK.Api.LogEvent("Delete Task");

                    tasks.ForEach(task => this._Delete(task));

                    if (navigateBack && page.NavigationService.CanGoBack)
                        page.NavigationService.GoBack();
                }
            });
        }

        // Mark task as deleted.
        private void _Delete(Task task)
        {
            // TODO: handle stop repeating.

            bool hasBeenSynced = task.TaskSeries.Source != "Mindsweep";

            if (hasBeenSynced)
            {
                task.Deleted = DateTime.UtcNow;
                task.TaskSeries.Modified = DateTime.UtcNow;
            }
            else
            {
                var requestsToDelete = mainDB.RequestQueue.Where(q => q.LocalTaskSeriesIdForAdd == task.TaskSeries.Id).ToList();
                var tasksToDelete = mainDB.Tasks.Where(t => t.TaskSeries.Id == task.TaskSeries.Id).ToList();
                mainDB.RequestQueue.DeleteAllOnSubmit(requestsToDelete);
                mainDB.Tasks.DeleteAllOnSubmit(tasksToDelete);
                mainDB.TaskSeries.DeleteOnSubmit(task.TaskSeries);
            }

            mainDB.SubmitChanges();

            LoadCollectionsFromDatabase();

            // If the task source is still mindsweep, it hasn't been synced with RTM yet. No need to request delete.
            if (hasBeenSynced)
            {
                QueueRequest(string.Format("{0}&list_id={1}&taskseries_id={2}&task_id={3}", RTM.URI_DELETETASK, task.TaskSeries.Project.Id, task.TaskSeries.Id, task.Id));
            }
        }

        public void ConfirmAndDeleteProject(Page page, Project project)
        {
            if (project.Locked)
            {
                RadMessageBox.Show("Cannot delete", MessageBoxButtons.OK, "Cannot delete a locked project.");
                return;
            }

            RadMessageBox.Show("Confirm delete", buttons: MessageBoxButtons.YesNo, message: "Are you sure you want to delete the \"" + project.Name + "\" project?", closedHandler: (args) =>
            {
                if (args.Result == DialogResult.OK)
                {
                    FlurryWP7SDK.Api.LogEvent("Delete Project");

                    this._DeleteProject(project);
                    page.NavigationService.Navigate(new Uri("/Views/MainPage.xaml", UriKind.RelativeOrAbsolute));
                }
            });
        }

        // Remove a project from the database and collections.
        private void _DeleteProject(Project projectForDelete)
        {
            // Remove the deck from the "all" observable collection.
            AllProjects.Remove(projectForDelete);

            // Remove the deck from the data context.
            mainDB.Projects.DeleteOnSubmit(projectForDelete);

            foreach (TaskSeries taskseries in projectForDelete.TaskSeries)
            {
                // TODO: task should be moved to inbox.
                taskseries.Tasks.ToList().ForEach(t => AllTasks.Remove(t));
                mainDB.TaskSeries.DeleteOnSubmit(taskseries);
            }

            // Save changes to the database.
            mainDB.SubmitChanges();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
        #endregion
    }
}
