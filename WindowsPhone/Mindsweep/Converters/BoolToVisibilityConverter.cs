using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Mindsweep.Converters
{
    public class BoolToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">If true, will give the opposite visibiliity.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool visibility = (bool)value;

            if (parameter != null && parameter.ToString().ToUpper() == bool.TrueString.ToUpper())
                visibility = !visibility;

            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            if (parameter != null && parameter.ToString().ToUpper() == bool.TrueString.ToUpper())
                return (visibility == Visibility.Visible);
            else
                return (visibility == Visibility.Visible);
        }
    }
}
