using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone;

namespace SanzaiWeibo.Control
{
    public static class ImageProperties
    {
        #region cache
        public static Dictionary<Int64, WeakReference<BitmapImage>> imageCache = new Dictionary<Int64, WeakReference<BitmapImage>>();

        #endregion

        #region SourceWithCustomReferer Property

        public static readonly DependencyProperty SourceWithCustomRefererProperty =
            DependencyProperty.RegisterAttached(
                "SourceWithCustomReferer",
                typeof(Uri),
                typeof(ImageProperties),
                new PropertyMetadata(OnSourceWithCustomRefererChanged));

        private static void OnSourceWithCustomRefererChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var image = (Image)o;
            var uri = (Uri)e.NewValue;

            if (image == null || uri == null)
                throw new ArgumentOutOfRangeException();

            if (DesignerProperties.IsInDesignTool)
            {
                // for the design surface we just load the image straight up
                image.Source = new BitmapImage(uri);
            }
            else
            {
                BitmapImage img = null;
                if (imageCache.ContainsKey(uri.GetHashCode()) && imageCache[uri.GetHashCode()].TryGetTarget(out img))
                {
                    image.Source = img;
                    return;
                }

                image.Source = null;

                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                request.Headers["Referer"] = "http://www.guokr.com"; // or your custom referer string here
                request.BeginGetResponse((result) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            using (Stream imageStream = request.EndGetResponse(result).GetResponseStream())
                            {
                                BitmapImage bitmapImage = new BitmapImage();
                                //bitmapImage.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                                bitmapImage.SetSource(imageStream);
                                image.Source = bitmapImage;
                                if (!imageCache.ContainsKey(uri.GetHashCode()))
                                    imageCache.Add(uri.GetHashCode(), new WeakReference<BitmapImage>(bitmapImage));
                            }
                        }
                        catch (WebException)
                        {
                            // add error handling
                        }
                    });
                }, null);
            }
        }

        public static Uri GetSourceWithCustomReferer(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("Image");
            }
            return (Uri)image.GetValue(SourceWithCustomRefererProperty);
        }

        public static void SetSourceWithCustomReferer(Image image, Uri value)
        {
            if (image == null)
            {
                throw new ArgumentNullException("Image");
            }
            image.SetValue(SourceWithCustomRefererProperty, value);
        }
        #endregion
    }
}
