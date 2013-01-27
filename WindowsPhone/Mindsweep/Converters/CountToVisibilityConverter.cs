using System;
using System.Windows;

namespace Mindsweep.Converters
{
    public class CountToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">If > 0, will result in visibiliity.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            int count = (int)value;

            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
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
