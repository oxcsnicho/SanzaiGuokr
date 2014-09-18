using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using SanzaiGuokr.Model;
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
    public class TileCacheManager
    {
        const string periodicTaskName = "GuokrUpdateTilePeriodicAgent";

        public static int ActiveTileCount
        {
            get
            {
                return ShellTile.ActiveTiles.Count();
            }
        }
        public static void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }

        public static void StartPeriodicAgent()
        {
            // Obtain a reference to the period task, if one exists
            var periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);

            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "换一下大瓷砖背面图片";

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);
#if DEBUG
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
#endif

                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("StartPeriodicAgent", "Result", "success", 1);
            }
            catch
            {
                UpdateTileScheduledTaskAgent.ScheduledAgent.ResetTile();
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("StartPeriodicAgent", "Result", "failure", 1);
            }
        }
        const string imageFolderPath = UpdateTileScheduledTaskAgent.ScheduledAgent.imageFolderPath;
        const string imagePrefix = UpdateTileScheduledTaskAgent.ScheduledAgent.imagePrefix;
        const string imageExt = UpdateTileScheduledTaskAgent.ScheduledAgent.imageExt;

        public static void StoreTileCache(recommend_article i)
        {
            if (i == null)
                return;
            var item = i as recommend_article;
            if (item == null)
                return;

            string imagePath = imageFolderPath + imagePrefix + item.id + imageExt;

            var sti = new TileCache();
            sti.title = item.title;
            sti.ImgSrc = item.ImgSrc;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream(imagePath, System.IO.FileMode.Create, store))
                    sti.GetCanvas().SaveJpeg(stream, 336, 336, 0, 100);
            }

        }

        internal static void ClearAllTilesCaches()
        {
#if !DEBUG
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var ii in store.GetFileNames(imageFolderPath + imagePrefix + "*" + imageExt))
                    store.DeleteFile(imageFolderPath + ii);
            }
#endif
        }

    }
    public class TileCache
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

        public WriteableBitmap GetCanvas()
        {
            int titleLength = title.Length + title.Where(c => c >= 0xFF && c != 0x201C && c != 0x201D).Count();
            if (titleLength > 22 && titleLength < 30 ||
                titleLength > 44 && titleLength < 50)
                textfactor = 0.85f;

            var textblock = new TextBlock()
            {
                Text = title,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Width = (int)(size - 2 * hoffset - 2 * textmargin) * textfactor,
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
