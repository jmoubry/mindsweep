using Mindsweep.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Windows;

namespace Mindsweep.Model
{
    [Table]
    public class TaskSeries : INotifyPropertyChanged
    {
        private string _id;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public string Id
        {
            get { return _id; }
            set
            {
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
                _tags = value;
                NotifyPropertyChanged("Tags");
                NotifyPropertyChanged("TagsList");
            }
        }

        public List<string> TagsList
        {
            get
            {
                List<string> lst = new List<string>();

                if (!string.IsNullOrEmpty(Tags))
                    lst.AddRange(Tags.Split(','));

                lst.Sort();

                return lst;
            }
        }

        private string _url;

        [Column]
        public string Url
        {
            get { return _url; }
            set
            {
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
                _project.Entity = value;

                if (value != null)
                {
                    _projectId = value.Id;
                }
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
            task.TaskSeries = this;
        }

        // Called during a remove operation
        private void detach_Task(Task task)
        {
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
            //get { return _repeatrule.HasLoadedOrAssignedValue ? _repeatrule.Entity : null; }
            get { return _repeatrule.Entity; }
            set
            {
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
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        #endregion
    }
}
