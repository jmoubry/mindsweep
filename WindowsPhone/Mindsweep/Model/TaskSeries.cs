using Mindsweep.Helpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Mindsweep.Model
{
    [Table]
    public class TaskSeries : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private string _id;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public string Id
        {
            get { return _id; }
            set
            {
                NotifyPropertyChanging("Id");
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        private string _name;

        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                NotifyPropertyChanging("Name");
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private DateTime _created;

        [Column]
        public DateTime Created
        {
            get { return _created; }
            set
            {
                NotifyPropertyChanging("Created");
                _created = value;
                NotifyPropertyChanged("Created");
            }
        }

        private DateTime _modified;

        [Column]
        public DateTime Modified
        {
            get { return _modified; }
            set
            {
                NotifyPropertyChanging("Modified");
                _modified = value;
                NotifyPropertyChanged("Modified");
            }
        }

        private string _source;

        [Column]
        public string Source
        {
            get { return _source; }
            set
            {
                NotifyPropertyChanging("Source");
                _source = value;
                NotifyPropertyChanged("Source");
            }
        }

        private string _tags;

        [JsonConverter(typeof(TagsToStringConverter))]
        [Column]
        public string Tags
        {
            get { return _tags; }
            set
            {
                NotifyPropertyChanging("Tags");
                _tags = value;
                NotifyPropertyChanged("Tags");
            }
        }

        private string _url;

        [Column]
        public string Url
        {
            get { return _url; }
            set
            {
                NotifyPropertyChanging("Url");
                _url = value;
                NotifyPropertyChanged("Url");
            }
        }

        // Internal column for the associated Project ID value
        [Column]
        internal string _projectId;

        // Entity reference, to identify the Project "storage" table
        private EntityRef<Project> _project;

        // Association, to describe the relationship between this key and that "storage" table
        [Association(Storage = "_project", ThisKey = "_projectId", OtherKey = "Id", IsForeignKey = true)]
        public Project Project
        {
            get { return _project.Entity; }
            set
            {
                NotifyPropertyChanging("Project");
                _project.Entity = value;

                if (value != null)
                {
                    _projectId = value.Id;
                }

                NotifyPropertyChanging("Project");
            }
        }


        // Assign handlers for the add and remove operations, respectively.
        public TaskSeries()
        {
            _tasks = new EntitySet<Task>(
                new Action<Task>(this.attach_Task),
                new Action<Task>(this.detach_Task)
                );
        }

        public void Sync(TaskSeries ts)
        {
            // Update if newer.
            if (ts.Modified > Modified)
            {
                Created = ts.Created;
                Modified = ts.Modified;
                Name = ts.Name;
                Project = ts.Project;
                RepeatRule = ts.RepeatRule;
                Source = ts.Source;
                Tags = ts.Tags;
                Tasks = ts.Tasks;
                Url = ts.Url;
            }
        }

        #region Child Tasks

        // Define the entity set for the collection side of the relationship.
        private EntitySet<Task> _tasks;

        [JsonProperty(PropertyName = "task")]
        [JsonConverter(typeof(ObjectToEntitySetConverter<Task>))]
        [Association(Storage = "_tasks", OtherKey = "_taskseriesId", ThisKey = "Id")]
        public EntitySet<Task> Tasks
        {
            get { return this._tasks; }
            set { this._tasks.Assign(value); }
        }

        // Called during an add operation
        private void attach_Task(Task task)
        {
            NotifyPropertyChanging("Task");
            task.TaskSeries = this;
        }

        // Called during a remove operation
        private void detach_Task(Task task)
        {
            NotifyPropertyChanging("Task");
            task.TaskSeries = null;
        }

        #endregion

        #region RepeatRule relationship

        // Internal column for the associated TaskSeries ID value
        [Column]
        internal int _repeatruleId;

        // Entity reference, to identify the TaskSeries "storage" table
        private EntityRef<RepeatRule> _repeatrule;

        // Association, to describe the relationship between this key and that "storage" table
        [JsonProperty(PropertyName = "rrule")]
        [Association(Storage = "_repeatrule", ThisKey = "_repeatruleId", OtherKey = "Id")]
        public RepeatRule RepeatRule
        {
            get { return _repeatrule.Entity; }
            set
            {
                NotifyPropertyChanging("RepeatRule");
                _repeatrule.Entity = value;

                if (value != null)
                {
                    _repeatruleId = value.Id;
                }

                NotifyPropertyChanged("RepeatRule");
            }
        }

        #endregion

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
}
