using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Text.RegularExpressions;
using System.Windows;

namespace Mindsweep.Model
{
    [Table]
    public class RepeatRule : INotifyPropertyChanged
    {
        private int _Id;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _Id; }
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private bool _every;

        [JsonConverter(typeof(Helpers.BitStringToBoolConverter))]
        [Column]
        public bool Every
        {
            get { return _every; }
            set
            {
                _every = value;
                NotifyPropertyChanged("Every");
            }
        }

        private string _rule;

        /*
            FREQ=MONTHLY;INTERVAL=1;BYDAY=-2FR
            FREQ=WEEKLY;INTERVAL=1;COUNT=20
            FREQ=MONTHLY;INTERVAL=1;BYMONTHDAY=4;UNTIL=20140101T000000"
            FREQ=DAILY;INTERVAL=2
            FREQ=YEARLY;INTERVAL=1
            FREQ=MONTHLY;INTERVAL=1;BYDAY=-1MO
        */
        [JsonProperty(PropertyName = "$t")]
        [Column]
        public string Rule
        {
            get { return _rule; }
            set
            {
                _rule = value;
                NotifyPropertyChanged("Rule");
                NotifyPropertyChanged("Frequency");
                NotifyPropertyChanged("Interval");
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("End");
                NotifyPropertyChanged("MonthlyByDayOfWeek");
                NotifyPropertyChanged("MonthlyByDayOfMonth");
                NotifyPropertyChanged("Description");
            }
        }

        public class ByDayOfWeek
        {
            public int Frequency { get; set; }
            public DayOfWeek Weekday { get; set; }
        }

        public ByDayOfWeek MonthlyByDayOfWeek
        {
            get
            {
                Match m = Regex.Match(Rule, @"BYDAY=(-?\d*)(\w\w)");

                if (m.Success && m.Groups.Count == 3)
                {
                    DayOfWeek day;

                    switch (m.Groups[2].Value)
                    {
                        case "SU": day = DayOfWeek.Sunday; break;
                        case "MO": day = DayOfWeek.Monday; break;
                        case "TU": day = DayOfWeek.Tuesday; break;
                        case "WE": day = DayOfWeek.Wednesday; break;
                        case "TH": day = DayOfWeek.Thursday; break;
                        case "FR": day = DayOfWeek.Friday; break;
                        case "SA": day = DayOfWeek.Saturday; break;
                        default: throw new NotImplementedException("DayOfWeek, " + m.Groups[2].Value + ", is not implemented.");
                    };

                    return new ByDayOfWeek()
                    {
                        Frequency = Convert.ToInt32(m.Groups[1].Value),
                        Weekday = day
                    };
                }

                return null;
            }
        }

        public int? MonthlyByDayOfMonth
        {
            get
            {
                Match m = Regex.Match(Rule, @"BYMONTHDAY=(\d*)");

                if (m.Success && m.Groups.Count == 2)
                    return Convert.ToInt32(m.Groups[1].Value);

                return null;
            }
        }

        public enum EndTypes
        {
            Never,
            Count,
            Date
        }

        public EndTypes End
        {
            get
            {
                if (Count.HasValue)
                    return EndTypes.Count;

                if (Until.HasValue)
                    return EndTypes.Date;

                return EndTypes.Never;
            }
        }

        public enum Frequencies
        {
            Daily,
            Weekly,
            Monthly,
            Yearly,
            Unknown
        }

        public string Description
        {
            get
            {
                string desc = "";

                desc = desc + (Every ? "every " : "after ");

                if (Interval >= 1)
                    desc = desc + Interval + " ";

                string s = Interval > 1 ? "s " : " ";

                switch(Frequency)
                {
                    case Frequencies.Daily: desc = desc + "day" + s; break;
                    case Frequencies.Weekly: desc = desc + "week" + s; break;
                    case Frequencies.Monthly: desc = desc + "month" + s; break;
                    case Frequencies.Yearly: desc = desc + "year" + s; break;
                }

                switch (End)
                {
                    case EndTypes.Count: desc = desc + "for " + Count.GetValueOrDefault() + " times"; break;
                    case EndTypes.Date: desc = desc + "until " + Until.GetValueOrDefault().ToShortDateString(); break;
                }

                return desc;
            }
        }

        public Frequencies Frequency
        {
            get
            {
                Match m = Regex.Match(Rule, "FREQ=([^;]*)");

                try
                {
                    if (m.Success && m.Groups.Count == 2)
                        return (Frequencies)Enum.Parse(typeof(Frequencies), m.Groups[1].Value, true);
                }
                catch (Exception)
                {

                }

                return Frequencies.Unknown;
            }
        }

        public int Interval
        {
            get
            {
                Match m = Regex.Match(Rule, @"INTERVAL=(\d*)");

                if (m.Success && m.Groups.Count == 2)
                    return Convert.ToInt32(m.Groups[1].Value);

                return 0;
            }
        }

        public int? Count
        {
            get
            {
                Match m = Regex.Match(Rule, @"COUNT=(\d*)");

                if (m.Success && m.Groups.Count == 2)
                    return Convert.ToInt32(m.Groups[1].Value);

                return null;
            }
        }

        public DateTime? Until
        {
            get
            {
                Match m = Regex.Match(Rule, @"UNTIL=(\d*)");

                if (m.Success && m.Groups.Count == 2)
                    return DateTime.ParseExact(m.Groups[1].Value, "yyyyMMdd", null);

                return null;
            }
        }


        #region Relation to TaskSeries

        // Internal column for the associated TaskSeries ID value
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
