using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SanzaiGuokr.Util
{
    public class SaveTileImage
    {
        const int size = 336;
        const int voffset = 0;
        const int hoffset = 0;
        const int textmargin = 12;
        float textfactor = 1;

        public BitmapImage ImgSrc { get; set; }
        public string title { get; set; }
        private Canvas ModifiedCanvas { get; set; }
        public string filename { get; set; }

        public WriteableBitmap CreateCanvas()
        {
            int titleLength = title.Length + title.Where(c => c >= 0xFF).Count();
            if (titleLength > 20 && titleLength < 30 ||
                titleLength > 40 && titleLength < 50)
                textfactor = 0.85f;

            var textblock = new TextBlock()
            {
                Text = title,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Width = (int)(size - 2 * hoffset - 2 * textmargin)*textfactor,
                Height = 40,
                MaxHeight = 40,
                TextAlignment = System.Windows.TextAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                FontSize = 28
            };
            var b = new Rectangle()
            {
                Width = size,
                Height = (int)(size - 2 * voffset) * 0.43 + voffset,
                Fill = new LinearGradientBrush()
                {
                    EndPoint = new System.Windows.Point(0.5, 1),
                    StartPoint = new System.Windows.Point(0.5, 0),
                    GradientStops = new GradientStopCollection()
                    {
                        new GradientStop() {Offset = 0},
                        new GradientStop() {Offset = 1, Color= Color.FromArgb(0xFF, 0x00, 0x00, 0x00)}
                    }
                }
            };
            var image = new Image()
            {
                Width = size,
                Height = size,
                Source = ImgSrc,
                Stretch = Stretch.UniformToFill
            };
            WriteableBitmap bm = new WriteableBitmap(size, size);
            bm.Render(image, null);
            bm.Render(b, new TranslateTransform()
            {
                X = 0,
                Y = size - voffset - b.ActualHeight
            });
            bm.Render(textblock, new TranslateTransform()
            {
                X = textmargin, //size - hoffset - textmargin - textblock.ActualWidth,
                Y = size - voffset - textmargin - textblock.ActualHeight
            });
            bm.Invalidate();
#if false
            var encoder = new PngEncoder();
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream("shared/shellcontent/" + filename + ".jpg", FileMode.Create, store))
                {
                    bm.SaveJpeg(stream, 336, 336, 0, 80);
                    stream.Close();
                }
            }
#endif

            return bm;
        }
    }
}
