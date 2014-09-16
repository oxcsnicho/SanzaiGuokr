using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using System.Linq;

namespace UpdateTileScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        public const string imageFolderPath = "Shared/ShellContent/";
        public const string imagePrefix = "Tile_";
        public const string imageExt = ".jpg";

        public static void UpdateTile()
        {
            // in debug mode, we don't update the tile as we check the tile through isolated storage
            string imagePath = null;
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var names = store.GetFileNames(imageFolderPath + imagePrefix + "*" + imageExt);
#if DEBUG
                    imagePath = imageFolderPath + names[(DateTime.Now.Minute) % names.Length]; // get the file round robin on a 30 minutes basis
#else
                    imagePath = imageFolderPath + names[(DateTime.Now.Minute / 30 + DateTime.Now.Hour * 2) % names.Length]; // get the file round robin on a 30 minutes basis
#endif
                }
            }
            catch
            {
                imagePath = null;
            }
            if (string.IsNullOrEmpty(imagePath))
                return;

            var flipTile = new FlipTileData();
            flipTile.BackTitle = "";
            flipTile.BackContent = "";
            flipTile.BackBackgroundImage = new Uri("isostore:/" + imagePath, UriKind.Absolute);

            InternalUpdateTile(flipTile);
        }
        private static void RevertTileUpdate()
        {
            var flipTile = new FlipTileData();
            flipTile.BackTitle = "";
            flipTile.BackContent = "";
            flipTile.BackBackgroundImage = null;
            InternalUpdateTile(flipTile);
        }
        private static void InternalUpdateTile(ShellTileData tile)
        {
            var t = ShellTile.ActiveTiles.FirstOrDefault();
            if (t == null)
                return;
            try
            {
                t.Update(tile);
            }
            catch
            {
            }
        }
        const string periodicTaskName = "UpdateTilePeriodicAgent";

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
            }
            catch
            {
                RevertTileUpdate();
            }
        }

        private static void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
#if DEBUG
            //TODO: Add code to perform your task in background
            string toastMessage = "";

            // If your application uses both PeriodicTask and ResourceIntensiveTask
            // you can branch your application code here. Otherwise, you don't need to.
            if (task is PeriodicTask)
            {
                // Execute periodic task actions here.
                toastMessage = "Periodic task running.";
            }
            else
            {
                // Execute resource-intensive task actions here.
                toastMessage = "Resource-intensive task running.";
            }

            // Launch a toast to show that the agent is running.
            // The toast will not be shown if the foreground application is running.
            ShellToast toast = new ShellToast();
            toast.Title = "Background Agent Sample";
            toast.Content = toastMessage;
            toast.Show();
#endif

            ScheduledAgent.UpdateTile();

#if DEBUG
            // If debugging is enabled, launch the agent again in one minute.
            ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
#endif

            // Call NotifyComplete to let the system know the agent is done working.
            NotifyComplete();
        }
    }
}