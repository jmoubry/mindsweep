using Mindsweep.Model;
using System;
using System.Windows;
using System.Windows.Media;

namespace Mindsweep.Converters
{
    public class TaskDueToSolidBrushConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            Task task = value as Task;

            if (task != null && task.Due.HasValue)
            {
                if (task.HasDueTime)
                {
                    if (task.Due.Value < DateTime.UtcNow)
                        return new SolidColorBrush(Color.FromArgb(255, 234, 82, 0));
                }
                else if (task.Due.Value.Date < DateTime.UtcNow.Date)
                    return new SolidColorBrush(Color.FromArgb(255, 234, 82, 0));
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
