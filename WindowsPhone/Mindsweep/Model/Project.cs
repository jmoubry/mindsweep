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
    public class Project : INotifyPropertyChanged
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

        private bool _deleted;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [Column]
        public bool Deleted
        {
            get { return _deleted; }
            set
            {
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
                _position = value;
                NotifyPropertyChanged("Position");
            }
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
            taskseries.Project = this;
        }

        // Called during a remove operation
        private void detach_TaskSeries(TaskSeries taskseries)
        {
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
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        #endregion
    }

    public class ProjectComparer : IEqualityComparer<Project>
    {
        public bool Equals(Project x, Project y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Project obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
