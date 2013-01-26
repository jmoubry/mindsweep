using Newtonsoft.Json;
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
        
        public string DueString
        {
            get
            {
                if (!Due.HasValue)
                    return string.Empty;
                else if (Due.Value.ToLocalTime().Date >= DateTime.Now.Date
                    && Due.Value.ToLocalTime() < DateTime.Now.Date.AddDays(7))
                {
                    return Due.Value.ToLocalTime().DayOfWeek.ToString();
                }
                else
                    return Due.Value.ToLocalTime().ToShortDateString();
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

        private bool _hasDueTime;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [JsonProperty(PropertyName="has_due_time")]
        [Column]
        public bool HasDueTime
        {
            get { return _hasDueTime; }
            set
            {
                NotifyPropertyChanging("HasDueTime");
                _hasDueTime = value;
                NotifyPropertyChanged("HasDueTime");
            }
        }


        private DateTime? _added;

        [Column]
        public DateTime? Added
        {
            get { return _added; }
            set
            {
                NotifyPropertyChanging("Added");
                _added = value;
                NotifyPropertyChanged("Added");
            }
        }

        private DateTime? _completed;

        [Column]
        public DateTime? Completed
        {
            get { return _completed; }
            set
            {
                NotifyPropertyChanging("Completed");
                _completed = value;
                NotifyPropertyChanged("Completed");
            }
        }

        private DateTime? _deleted;

        [Column]
        public DateTime? Deleted
        {
            get { return _deleted; }
            set
            {
                NotifyPropertyChanging("Deleted");
                _deleted = value;
                NotifyPropertyChanged("Deleted");
            }
        }

        private int _postponed;

        [Column]
        public int Postponed
        {
            get { return _postponed; }
            set
            {
                NotifyPropertyChanging("Postponed");
                _postponed = value;
                NotifyPropertyChanged("Postponed");
            }
        }

        private char _priority;

        [Column]
        public char Priority
        {
            get { return _priority; }
            set
            {
                NotifyPropertyChanging("Priority");
                _priority = value;
                NotifyPropertyChanged("Priority");
            }
        }


        private string _estimate;

        [Column]
        public string Estimate
        {
            get { return _estimate; }
            set
            {
                NotifyPropertyChanging("Estimate");
                _estimate = value;
                NotifyPropertyChanged("Estimate");
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
