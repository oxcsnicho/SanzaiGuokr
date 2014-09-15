using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SanzaiGuokrV8.Resources;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.Util;
using System.Collections.Generic;
using Microsoft.Phone.Net.NetworkInformation;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SanzaiGuokr.Model;
using Microsoft.Phone.Tasks;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;

namespace SanzaiGuokrV8
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            // Resource Dictionaries
            var fontDict = new ResourceDictionary() { Source = new Uri("/SanzaiGuokrV8;component/Styles/FontSize" + ViewModelLocator.ApplicationSettingsStatic.FontSizeSettingEnum.ToString() + ".xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(fontDict);
            var themeDict = new ResourceDictionary() { Source = new Uri("/SanzaiGuokrV8;component/Styles/" + ViewModelLocator.ApplicationSettingsStatic.ColorThemeStatus.ToString() + ".xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(themeDict);
            var commonDict = new ResourceDictionary() { Source = new Uri("/SanzaiGuokrV8;component/Styles/Common.xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(commonDict);

            RootFrame.Navigated += RootFrame_Navigated;
        }

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView(e.Uri.ToString());
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            ResumeUsage();
            Common.CheckNetworkStatus();

            DeviceNetworkInformation.NetworkAvailabilityChanged += DeviceNetworkInformation_NetworkAvailabilityChanged;

#if PIPROFILING
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (ss, ee) =>
            {
                //long deviceTotalMemory = (long)Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceTotalMemory");
                long applicationCurrentMemoryUsage = (long)Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
                long applicationPeakMemoryUsage = (long)Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage");
                Messenger.Default.Send<SetProgressIndicator>(new SetProgressIndicator()
                {
                    Text = string.Format("M: {0}M/{1}M",
                           (applicationCurrentMemoryUsage / 100000) / (float)10,
                           (applicationPeakMemoryUsage / 100000) / (float)10),
                    IsStick = true
                });
            };
            timer.Start();
#endif
        }

        void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            Common.CheckNetworkStatus();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            //Api.StartSession("6676FNCYNHJ2Z8CK6VZG");
            ResumeUsage();
            Common.CheckNetworkStatus();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            StopUsage();
            ViewModelLocator.BookmarkStatic.BookmarkList.Bookmarks.SubmitChanges();
            var AS = IsolatedStorageSettings.ApplicationSettings;
            if(!RootFrame.CurrentSource.ToString().Contains("FileTypeAssociation"))
                if (AS.Contains("lastUri"))
                    AS["lastUri"] = RootFrame.CurrentSource;
                else
                    AS.Add("lastUri", RootFrame.CurrentSource);
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            ViewModelLocator.BookmarkStatic.BookmarkList.Bookmarks.SubmitChanges();
            ReportUsage();

            if (ViewModelLocator.MainStatic.RecommendedList.Status == SanzaiGuokr.Model.StatusType.SUCCESS)
#if DEBUG
                foreach (var item in ViewModelLocator.MainStatic.RecommendedArticles)
                    StoreTile(item);
#else
                StoreTile();
#endif
            ViewModelLocator.Cleanup();
        }

        private void StoreTile(recommend_article i = null)
        {
            if(i==null)
                i = ViewModelLocator.MainStatic.RecommendedArticles.FirstOrDefault();
            if(i==null)
                return;
            var item =i as recommend_article;
            if(item == null)
                return;

            string imageFolderPath = "Shared/ShellContent/";
            string imagePrefix = "Tile_";
            string imageExt = ".jpg";
            string imagePath = imageFolderPath + imagePrefix + item.id + imageExt;

            var sti = new SaveTileImage();
            sti.title = item.title;
            sti.ImgSrc = item.ImgSrc;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
#if !DEBUG
                foreach (var ii in store.GetFileNames(imageFolderPath + imagePrefix + "*" + imageExt))
                    store.DeleteFile(imageFolderPath + ii);
#endif

                using (var stream = new IsolatedStorageFileStream(imagePath, System.IO.FileMode.Create, store))
                    sti.CreateCanvas().SaveJpeg(stream, 336, 336, 0, 100);
            }

#if !DEBUG
            // in debug mode, we don't update the tile as we check the tile through isolated storage
            var flipTile = new FlipTileData();
            flipTile.Title = "";
            flipTile.SmallBackgroundImage = new Uri("/guokr_159x159.png", UriKind.Relative);
            flipTile.BackTitle = "";
            flipTile.BackContent = "";
            flipTile.BackBackgroundImage = new Uri("isostore:/" + imagePath, UriKind.Absolute);

            if (ShellTile.ActiveTiles.Count() == 0)
                ShellTile.Create(new Uri("/MainPage.xaml", UriKind.Relative), flipTile, false);
            else
            {
                var tile = ShellTile.ActiveTiles.First();
                try
                {
                    tile.Update(flipTile);
                }
                catch
                {
                    tile.Delete();
                    ShellTile.Create(new Uri("/MainPage.xaml", UriKind.Relative), flipTile, false);
                }
            }
#endif

        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (e.Exception.Message.IndexOf("NullRef", StringComparison.OrdinalIgnoreCase) >= 0)
                MessageBox.Show("谢谢使用！再见！");
            else
                MessageBox.Show(e.Exception.Message);
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject.Message.IndexOf("NullRef", StringComparison.OrdinalIgnoreCase) >= 0)
                MessageBox.Show("谢谢使用！再见！");
            else
                MessageBox.Show(e.ExceptionObject.Message);
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // WeiXin integration
            RootFrame.UriMapper = new SanzaiGuokrV8.Util.AssociationUriMapper();

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }


        static string lastname;
        static DateTime lasttime = DateTime.Now;
        public static void ReportUsage(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = lastname;

            var diff = DateTime.Now - lasttime;
#if !DEBUG
            if (diff > TimeSpan.FromSeconds(3))
#endif
                GoogleAnalytics.EasyTracker.GetTracker().SendTiming(diff, "PivotSwitch", "AwaitTime", name);
            lastname = name;
            lasttime = DateTime.Now;
        }
        public static void StopUsage()
        {
            ReportUsage();
        }
        public static void ResumeUsage()
        {
            lasttime = DateTime.Now;
        }
    }
}