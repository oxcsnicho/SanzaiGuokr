using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;

namespace SanzaiGuokr.Converters
{
    public class ConditionalAlertColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return System.Convert.ToBoolean(value)
                ? Application.Current.Resources["DefaultGreen"]
                : Application.Current.Resources["DefaultSubtle"];
            else
                return System.Convert.ToBoolean(value)
                ? parameter
                : (parameter.GetType() == typeof(Color) ? Application.Current.Resources["DefaultSubtle"]
                            : Application.Current.Resources["DefaultSubtleBrush"]);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
