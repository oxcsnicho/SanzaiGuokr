using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Reflection;

namespace SanzaiGuokr
{
    public partial class about : PhoneApplicationPage
    {
        public about()
        {
            InitializeComponent();
        }

        private void contact_support(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();
            task.Subject = string.Format("[SanzaiGuokr, ver {0}] suggestion/comments", GetVersionNumber());
            task.To = "sanzaiweibo@gmail.com";
            task.Body = "Please input your feature request below:" + Environment.NewLine;
            task.Show();
        }

        public static string GetVersionNumber()
        {
            var asm = Assembly.GetExecutingAssembly();
            var parts = asm.FullName.Split(',');
            return parts[1].Split('=')[1];
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://weibo.com/oxcsnicho", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void rate_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("求好评啊，亲！\n有话好商量啊亲！\n可以发email骚扰啊亲！\n给差评俺没法回啊亲！\n给条活路啊亲！\n\n ○(┬﹏┬)○");
            var t = new MarketplaceReviewTask();
            t.Show();
        }

    }
}
