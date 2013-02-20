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
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed;
            else
                return System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible);
        }
    }
    public class ConditionalAlertColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value)
            ? Application.Current.Resources["DefaultGreen"]
            : Application.Current.Resources["DefaultSubtle"];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int ret = 0;
            try
            {
                ret = System.Convert.ToInt32(value);
            }
            catch { }
            return ret;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SanzaiGuokr.ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.LARGE;
            if (value.GetType() == typeof(int))
            {
                int i = (int)value;
                if (i == 0)
                    val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.NORMAL;
                else if (i == 1)
                    val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.LARGE;
                else if (i == 2)
                    val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.EXTRALARGE;
            }
            return val;
        }
    }
}
