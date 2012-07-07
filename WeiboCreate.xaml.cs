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

namespace SanzaiWeibo.Pages
{
    public partial class EditWeibo : PhoneApplicationPage
    {
        private SinaApi _base = SinaApi.base_oauth;
        private PhotoChooserTask _task = new PhotoChooserTask();
        PinyinHelper py = PinyinHelper.Default;
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
            article a = ViewModelLocator.ReadArticleStatic.the_article;
            textBox1.Text = string.Format(" //分享@果壳网 文章:{0} {1} (来自@山寨果壳)", a.title, a.FullUrl);
            image_preview.Source = new BitmapImage(new Uri(a.pic, UriKind.Absolute));
        }

        #region post weibo
        private void post_weibo(object sender, RoutedEventArgs e)
        {

            curtain.Visibility = System.Windows.Visibility.Visible;
            /*
             * don't enable posting pic at this moment
             */
            if (null == image_preview.Source)
            {
                var api_update = new SinaApi.Api_Update();
                api_update.call_complete += new EventHandler<SinaApi.ApiResultEventArgs<status>>(post_weibo_complete);
                api_update.call(textBox1.Text);
            }
            else
            {
                var api_upload = new SinaApi.Api_Upload();
                api_upload.call_complete+=new EventHandler<SinaApi.ApiResultEventArgs<status>>(post_weibo_complete);

                api_upload.call(textBox1.Text, ImageToByteArray(image_preview.Source as BitmapImage), photo_filename);
            }

            if (NavigationService.CanGoBack)
                try
                {
                    NavigationService.GoBack();
                }
                catch
                {
                }

            //pi.set("sending...");

        }

        void post_weibo_complete(object sender, SinaApi.ApiResultEventArgs<status> e)
        {
#if FALSE
            if (e.is_success == false)
                pi.message(e.Error.error);
            else
                pi.message("发布成功！");
#endif
        }

        #endregion

        #region add photo

        public Byte[] ImageToByteArray(BitmapImage bitmapImage)
        {
            byte[] data = null;

            using (MemoryStream stream = new MemoryStream())
            {
                WriteableBitmap wBitmap = new WriteableBitmap(bitmapImage);
                var width = wBitmap.PixelWidth / 2;
                var height = wBitmap.PixelHeight / 2;
                wBitmap.SaveJpeg(stream, width, height, 0, 85);
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

#if FALSE
        #region progress indicator
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
        #endregion
#endif

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