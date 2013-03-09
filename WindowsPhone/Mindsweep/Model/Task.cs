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
    public class Task : INotifyPropertyChanged
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

        public string DueStringLong
        {
            get
            {
                if (!Due.HasValue)
                    return "No due date";
                else if (Due.Value.ToLocalTime().Year == DateTime.Now.Year)
                    return string.Format("{0:ddd M/d}{1}", Due.Value.ToLocalTime(), HasDueTime ? ", " + Due.Value.ToLocalTime().ToShortTimeString() : string.Empty);
                else
                    return string.Format("{0:ddd M/d/yyyy}{1}", Due.Value.ToLocalTime(), HasDueTime ? ", " + Due.Value.ToLocalTime().ToShortTimeString() : string.Empty);
            }
        }
        
        public string DueString
        {
            get
            {
                if (!Due.HasValue)
                    return string.Empty;
                else if (Due.Value.ToLocalTime().Date == DateTime.Now.Date)
                    return "Today";
                else if (Due.Value.ToLocalTime().Date >= DateTime.Now.Date
                    && Due.Value.ToLocalTime() < DateTime.Now.Date.AddDays(7))
                    return Due.Value.ToLocalTime().DayOfWeek.ToString();
                else if (Due.Value.ToLocalTime().Date.Year == DateTime.Now.Date.Year)
                    return Due.Value.ToLocalTime().ToString("MMM d");
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
                _postponed = value;
                NotifyPropertyChanged("Postponed");
            }
        }

        private string _priority;

        [Column]
        public string Priority
        {
            get { return _priority; }
            set
            {
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
                _estimate = value;
                NotifyPropertyChanged("Estimate");
            }
        }


        private string _projectNameHack;
        public string ProjectName
        {
            get { return this._projectNameHack; }
            set
            {
                this._projectNameHack = value;
                NotifyPropertyChanged("ProjectName");
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
                _taskseries.Entity = value;

                if (value != null)
                {
                    _taskseriesId = value.Id;
                }
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
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
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
