using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Controls;
using SanzaiGuokr.SinaApiV2;
using SanzaiGuokr.ViewModel;
using SanzaiWeibo.Utils;
using WeiboApi;

namespace SanzaiWeibo.Pages
{
    public partial class RepostWeibo : PhoneApplicationPage
    {
        PinyinHelper py = PinyinHelper.Default;
        public status a
        {
            get
            {
                return ViewModelLocator.MainStatic.the_weibo;
            }
        }
        public RepostWeibo()
        {
            InitializeComponent();
        }
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (a != null)
            {
                textBox1.Text = a.retweeted_status == null ? "" : "//" + a.name_and_text;
            }

            sending_popup.Visibility = System.Windows.Visibility.Collapsed;
        }

        int TextLength(string s)
        {
            double sum = 0;
            foreach (var c in s)
                sum += (int)c > 127 ? 1 : 0.5;
            return (int)(sum + 0.5);
        }

        #region post weibo
        private async void post_weibo(object sender, RoutedEventArgs e)
        {
            if (TextLength(textBox1.Text) > 140)
                textBox1.Text = textBox1.Text.Substring(0, 138 - 2) + "...";

            Task<WeiboApi.status> t = null;
                t = SinaApiV2.RepostWeibo(textBox1.Text, a);

            TurnOnSendingPopup();
            sending_notification.Text = "正在发送";

            try
            {
                await t;
                MessageBox.Show("发送成功!");
                sending_progress.IsIndeterminate = false;
                sending_progress.Visibility = System.Windows.Visibility.Collapsed;
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (SinaWeiboException ee)
            {
                MessageBox.Show("发送失败.. " + ee.error);
                sending_progress.IsIndeterminate = false;
                sending_progress.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch
            {
                MessageBox.Show("出bug了");
            }
        }
        #endregion

        #region progress indicator
#if FALSE
        MyProgressIndicator pi = new MyProgressIndicator();
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            pi.reset();
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            pi.set("Loading");
            base.OnNavigatedFrom(e);
        }
#endif
        void TurnOnSendingPopup()
        {
            sending_popup.IsOpen = true;
            sending_notification.Text = "正在发送";
            sending_progress.IsIndeterminate = true;
            sending_progress.Visibility = System.Windows.Visibility.Visible;
            sending_popup.Visibility = System.Windows.Visibility.Visible;
        }
        void TurnOffSendingPopup()
        {
            sending_popup.IsOpen = false;
            sending_progress.IsIndeterminate = false;
            sending_progress.Visibility = System.Windows.Visibility.Collapsed;
            sending_popup.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

    }

}