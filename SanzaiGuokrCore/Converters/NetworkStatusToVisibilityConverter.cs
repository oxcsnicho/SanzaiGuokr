﻿using System;
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
    public class NetworkStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (SanzaiGuokr.ViewModel.ApplicationSettingsViewModel.NetworkType)value == ViewModel.ApplicationSettingsViewModel.NetworkType.WIFI
                ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible) ? SanzaiGuokr.ViewModel.ApplicationSettingsViewModel.NetworkType.WIFI : ViewModel.ApplicationSettingsViewModel.NetworkType.CELLULAR;
        }
    }
}
