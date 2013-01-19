using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Mindsweep.Model
{
    [Table]
    public class Project : INotifyPropertyChanged, INotifyPropertyChanging
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

        private bool _deleted;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [Column]
        public bool Deleted
        {
            get { return _deleted; }
            set
            {
                NotifyPropertyChanging("Deleted");
                _deleted = value;
                NotifyPropertyChanged("Deleted");
            }
        }

        private bool _locked;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [Column]
        public bool Locked
        {
            get { return _locked; }
            set
            {
                NotifyPropertyChanging("Locked");
                _locked = value;
                NotifyPropertyChanged("Locked");
            }
        }

        private bool _archived;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [Column]
        public bool Archived
        {
            get { return _archived; }
            set
            {
                NotifyPropertyChanging("Archived");
                _archived = value;
                NotifyPropertyChanged("Archived");
            }
        }

        private bool _smart;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [Column]
        public bool Smart
        {
            get { return _smart; }
            set
            {
                NotifyPropertyChanging("Smart");
                _smart = value;
                NotifyPropertyChanged("Smart");
            }
        }

        private int _position;

        [Column]
        public int Position
        {
            get { return _position; }
            set
            {
                NotifyPropertyChanging("Position");
                _position = value;
                NotifyPropertyChanged("Position");
            }
        }

        public void Sync(Project p)
        {
            Name = p.Name;
            Deleted = p.Deleted;
            Locked = p.Locked;
            Archived = p.Archived;
            Smart = p.Smart;
            Position = p.Position;
        }

        // Define the entity set for the collection side of the relationship.
        private EntitySet<TaskSeries> _taskseries;

        [Association(Storage = "_taskseries", OtherKey = "_projectId", ThisKey = "Id")]
        public EntitySet<TaskSeries> TaskSeries
        {
            get { return this._taskseries; }
            set { this._taskseries.Assign(value); }
        }

        // Assign handlers for the add and remove operations, respectively.
        public Project()
        {
            _taskseries = new EntitySet<TaskSeries>(
                new Action<TaskSeries>(this.attach_TaskSeries),
                new Action<TaskSeries>(this.detach_TaskSeries)
                );
        }

        // Called during an add operation
        private void attach_TaskSeries(TaskSeries taskseries)
        {
            NotifyPropertyChanging("TaskSeries");
            taskseries.Project = this;
        }

        // Called during a remove operation
        private void detach_TaskSeries(TaskSeries taskseries)
        {
            NotifyPropertyChanging("TaskSeries");
            taskseries.Project = null;
        }

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
