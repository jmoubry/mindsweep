using System;
using System.Windows;
using System.Windows.Media;

namespace Mindsweep.Converters
{
    public class PriorityToSolidBrushConverter : System.Windows.Data.IValueConverter
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
            string priority = value as string;

            if (string.IsNullOrWhiteSpace(priority))
                return new SolidColorBrush(Colors.LightGray);

            switch (priority)
            {
                case "1": return new SolidColorBrush(Color.FromArgb(255, 234, 82, 0));
                case "2": return new SolidColorBrush(Color.FromArgb(255, 0, 96, 191));
                case "3": return new SolidColorBrush(Color.FromArgb(255, 53, 154, 255));
                default: return new SolidColorBrush(Colors.LightGray); 
            };
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
