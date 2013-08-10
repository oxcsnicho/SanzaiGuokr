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
using System.Globalization;
using System.Windows.Data;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.Converters
{
    public class StatusTypeToFooterTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (StatusType) System.Convert.ToInt32(value);
            switch(s)
            {
                case StatusType.NOTLOADED:
                    if (parameter as string == "search")
                        return Application.Current.Resources["PromptSearchFooterTamplate"];
                    else
                        return null;
                case StatusType.SUCCESS:
                    if (parameter as string == "123")
                        return Application.Current.Resources["RefreshFooterTamplate"];
                    else return null;
                case StatusType.INPROGRESS:
                    return Application.Current.Resources["LongListSelectorFooter"];
                case StatusType.FAILED:
                    return Application.Current.Resources["FailedFooterTamplate"];
                case StatusType.ENDED:
                    return Application.Current.Resources["EndedFooterTamplate"];
                case StatusType.UNDERCONSTRUCTION:
                    return Application.Current.Resources["UnderConstructionFooterTamplate"];
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible);
        }
    }
}
