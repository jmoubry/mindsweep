using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Windows;

namespace Mindsweep.Model
{
    [Table]
    public class Request : INotifyPropertyChanged
    {
        private int _id;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false)]
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        private string _localTaskSeriesIdForAdd;

        [Column(CanBeNull = true)]
        public string LocalTaskSeriesIdForAdd
        {
            get { return _localTaskSeriesIdForAdd; }
            set
            {
                _localTaskSeriesIdForAdd = value;
                NotifyPropertyChanged("LocalTaskSeriesIdForAdd");
            }
        }

        private string _requestUri;

        [Column]
        public string RequestUri
        {
            get { return _requestUri; }
            set
            {
                _requestUri = value;
                NotifyPropertyChanged("RequestUri");
            }
        }

        private DateTime? _requested;

        [Column]
        public DateTime? Requested
        {
            get { return _requested; }
            set
            {
                _requested = value;
                NotifyPropertyChanged("Requested");
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
}
