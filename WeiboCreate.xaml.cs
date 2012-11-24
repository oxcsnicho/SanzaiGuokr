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
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Phone.Shell;
using System.Text;
using WeiboApi;
using SanzaiWeibo.Utils;
using SanzaiGuokr.Model;
using SanzaiGuokr.ViewModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.Threading.Tasks;
using RestSharp;
using SanzaiGuokr.SinaApiV2;

namespace SanzaiWeibo.Pages
{
    public partial class EditWeibo : PhoneApplicationPage
    {
        private SinaApi _base = SinaApi.base_oauth;
        private PhotoChooserTask _task = new PhotoChooserTask();
        PinyinHelper py = PinyinHelper.Default;
        public article a
        {
            get
            {
                return ViewModelLocator.ReadArticleStatic.the_article;
            }
        }
        public EditWeibo()
        {
            InitializeComponent();
            _task.Completed += new EventHandler<PhotoResult>(_task_Completed);

            #region atpeople
            //atpeople_completebox.ItemsSource = UserManager.Manager.Friends;
            atpeople_completebox.TextFilter = new AutoCompleteFilterPredicate<string>(is_matched);
            #endregion
        }
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (a != null)
            {
                textBox1.Text = string.Format(" //@果壳网:{0} {1} {2}", a.title, a.FullUrl, a.Abstract);
                string large_pic_uri = a.pic.Replace("/img2.", "/img1.").Replace("thumbnail", "gkimage").Replace("_90", "");

                var large_uri = new Uri(large_pic_uri, UriKind.Absolute);
                var _imgsrc = new BitmapImage();
                WebClient wc = new WebClient();
                wc.Headers["Referer"] = "http://www.guokr.com";
                wc.OpenReadCompleted += (s, ee) =>
                    {
                        try
                        {
                            _imgsrc.SetSource(ee.Result);
                        }
                        catch
                        {

                        }
                    };
                wc.OpenReadAsync(large_uri);
                _imgsrc.ImageFailed += (ss, ee) => _imgsrc.UriSource = new Uri(a.pic, UriKind.Absolute);
                image_preview.Source = _imgsrc;
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
            if (null == image_preview.Source)
                t = SinaApiV2.PostWeibo(textBox1.Text);
            else
                t = SinaApiV2.PostWeibo(textBox1.Text, ImageToByteArray(image_preview.Source as BitmapImage));

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

#if false
        void post_weibo_complete()
        {
            if (e.is_success == false)
            {
                var dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(SendingNotificationDisplayTime);
                dt.Start();
                dt.Tick += new EventHandler((ss, ee) =>
                {
                    TurnOffSendingPopup();
                    dt.Stop();
                });
            }
            else
            {
                var dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(SendingNotificationDisplayTime);
                dt.Start();
                dt.Tick += new EventHandler((ss, ee) =>
                {
                    if (NavigationService.CanGoBack)
                        try
                        {
                            NavigationService.GoBack();
                        }
                        catch
                        {
                        }
                    dt.Stop();
                });
            }
        }
#endif

        #endregion

        #region add photo

        public Byte[] ImageToByteArray(BitmapImage bitmapImage)
        {
            byte[] data = null;

            using (MemoryStream stream = new MemoryStream())
            {
                WriteableBitmap wBitmap = new WriteableBitmap(bitmapImage);
                var width = wBitmap.PixelWidth;
                var height = wBitmap.PixelHeight;
                wBitmap.SaveJpeg(stream, width, height, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);
                data = stream.GetBuffer();

                size_info.Text = string.Format("size: {0} KB, resolution: {1}x{2}",
                    (data.Length / 1024).ToString(),
                    width,
                    height);
            }
            return data;
        }

        private void btn_photo_Click(object sender, RoutedEventArgs e)
        {
            _task.ShowCamera = true;
            _task.Show();
        }

        string photo_filename = null;
        System.IO.Stream photo_stream = null;
        BitmapImage photo_img = null;
        byte[] photo_byte;
        void _task_Completed(object sender, PhotoResult e)
        {
            if (e.OriginalFileName == null)
                return;
            photo_img = new BitmapImage(new Uri(e.OriginalFileName));
            image_preview.Source = photo_img;
            photo_filename = e.OriginalFileName;
            photo_stream = e.ChosenPhoto;
            // extract exif
            // resize to control size
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

        #region atpeople
        private void btn_atpeople_click(object sender, System.Windows.RoutedEventArgs e)
        {
            atpeople_popup.IsOpen = true;
            atpeople_completebox.Focus();
        }

        private void atpeople_button_click(object sender, System.Windows.RoutedEventArgs e)
        {
            atpeople_popup.IsOpen = false;
            textBox1.text_box.SelectedText = "@" + atpeople_completebox.Text + " ";
            //UserManager.Manager.AddCoreFriend(atpeople_completebox.Text);
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (atpeople_popup.IsOpen == true)
            {
                atpeople_popup.IsOpen = false;
                e.Cancel = true;
                return;
            }
            base.OnBackKeyPress(e);
        }

        private void atpeople_popup_opened(object sender, System.EventArgs e)
        {
            atpeople_completebox.Text = "";
        }
        bool is_matched(string search, string value)
        {
            return py.ToPyString(value).Contains(search);
        }

        private void photo_boarder_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            btn_photo_Click(sender, new RoutedEventArgs());
        }


        #endregion



    }

}