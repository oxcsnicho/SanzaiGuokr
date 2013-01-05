using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Threading.Tasks;

namespace SanzaiGuokr
{
    public partial class ImagePopupViewer : UserControl
    {
        BitmapImage bi = new BitmapImage() { CreateOptions = BitmapCreateOptions.BackgroundCreation };
        public ImagePopupViewer()
        {
            // Required to initialize variables
            InitializeComponent();

            bi.DownloadProgress += (ss, ee) => Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if(ee.Progress != 100)
                        counter.Text = ee.Progress.ToString() + "%";
                    progress.Value = ee.Progress;
                });
            bi.ImageFailed += (ss, ee) => Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    VisualStateManager.GoToState(this, "Downloading", false);
                    counter.Text = "图片貌似打不开";
                    progress.Value = 0;
                });

            bi.ImageOpened += (ss, ee) => Deployment.Current.Dispatcher.BeginInvoke(() => 
                {
                    VisualStateManager.GoToState(this, "LoadComplete", false);
                    try
                    {
                        popup_image.Source = bi;
                    }
                    catch
                    {
                        VisualStateManager.GoToState(this, "Downloading", false);
                        counter.Text = "图片貌似打不开";
                        progress.Value = 0;
                    }
                });
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Downloading", false);
            counter.Text = "0%";
            this.scrollViewer.ScrollToVerticalOffset(0);
        }

        #region SourceUri
        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.Register(
            "SourceUri", typeof(Uri), typeof(ImagePopupViewer),
            new PropertyMetadata(default(Uri), new PropertyChangedCallback(SourceUriChanged))
            );

        public Uri SourceUri
        {
            get
            {
                return (Uri)GetValue(SourceUriProperty);
            }
            set
            {
                SetValue(SourceUriProperty, value);
            }
        }

        enum FileType
        {
            NONGIF,
            GIF
        };
        FileType fileType;
        static void SourceUriChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var current = sender as ImagePopupViewer;
            var value = e.NewValue as Uri;
            if (current == null || value == null)
                return;

#if false
            var path = value.AbsolutePath;
            if (path.Length > 3 && string.Compare(path.Substring(path.Length - 3), "gif", StringComparison.InvariantCultureIgnoreCase) == 0)
                current.fileType = FileType.GIF;
            else
                current.fileType = FileType.NONGIF;
#endif

            VisualStateManager.GoToState(current, "Downloading", false);
            current.counter.Text = "0%";
            current.bi.UriSource = value;
        }

        #endregion

        #region ProgressCounter
        public static readonly DependencyProperty ProgressCounterProperty =
            DependencyProperty.Register(
            "ProgressCounter", typeof(Uri), typeof(ImagePopupViewer),
            new PropertyMetadata(default(Uri), null)
            );

        private int _pgCounter;

        public int ProgressCounter
        {
            get { return _pgCounter; }
            private set { _pgCounter = value; }
        }

        #endregion

    }
}