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
            if (value.GetType() == typeof(int))
            {
                if (parameter == null)
                {
                    int i = (int)value;
                    var val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.Large;
                    if (i == 0)
                        val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.Normal;
                    else if (i == 1)
                        val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.Large;
                    else if (i == 2)
                        val = ViewModel.ApplicationSettingsViewModel.FontSizeSettingValue.ExtraLarge;
                    return val;
                }
                else
                {
                    int i = (int)value;
		    var val = ViewModel.ApplicationSettingsViewModel.ColorThemeOptions.Auto;
                    if (i == 0)
                        val = ViewModel.ApplicationSettingsViewModel.ColorThemeOptions.Auto;
                    else if (i == 1)
                        val = ViewModel.ApplicationSettingsViewModel.ColorThemeOptions.AlwaysDay;
                    else if (i == 2)
                        val = ViewModel.ApplicationSettingsViewModel.ColorThemeOptions.AlwaysNight;
                    return val;
                }

            }
            return null;
        }
    }
}
