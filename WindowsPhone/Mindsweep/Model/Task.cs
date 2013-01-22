using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Mindsweep.Model
{
    [Table]
    public class Task : INotifyPropertyChanged, INotifyPropertyChanging
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

        private DateTime? _due;

        [Column]
        public DateTime? Due
        {
            get { return _due; }
            set
            {
                NotifyPropertyChanging("Due");
                _due = value;
                NotifyPropertyChanged("Due");
            }
        }

        // Internal column for the associated Project ID value
        [Column]
        internal string _taskseriesId;

        // Entity reference, to identify the TaskSeries "storage" table
        private EntityRef<TaskSeries> _taskseries;

        // Association, to describe the relationship between this key and that "storage" table
        [Association(Storage = "_taskseries", ThisKey = "_taskseriesId", OtherKey = "Id", IsForeignKey = true)]
        public TaskSeries TaskSeries
        {
            get { return _taskseries.Entity; }
            set
            {
                NotifyPropertyChanging("TaskSeries");
                _taskseries.Entity = value;

                if (value != null)
                {
                    _taskseriesId = value.Id;
                }

                NotifyPropertyChanging("TaskSeries");
            }
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

    public class TaskComparer : IEqualityComparer<Task>
    {
        public bool Equals(Task x, Task y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Task obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
