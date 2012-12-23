/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SanzaiGuokr.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  
  OR (WPF only):
  
  xmlns:vm="clr-namespace:SanzaiGuokr.ViewModel"
  DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
*/

namespace SanzaiGuokr.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// Use the <strong>mvvmlocatorproperty</strong> snippet to add ViewModels
    /// to this locator.
    /// </para>
    /// <para>
    /// In Silverlight and WPF, place the ViewModelLocatorTemplate in the App.xaml resources:
    /// </para>
    /// <code>
    /// &lt;Application.Resources&gt;
    ///     &lt;vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SanzaiGuokr.ViewModel"
    ///                                  x:Key="Locator" /&gt;
    /// &lt;/Application.Resources&gt;
    /// </code>
    /// <para>
    /// Then use:
    /// </para>
    /// <code>
    /// DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
    /// </code>
    /// <para>
    /// You can also use Blend to do all this with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// <para>
    /// In <strong>*WPF only*</strong> (and if databinding in Blend is not relevant), you can delete
    /// the Main property and bind to the ViewModelNameStatic property instead:
    /// </para>
    /// <code>
    /// xmlns:vm="clr-namespace:SanzaiGuokr.ViewModel"
    /// DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
    /// </code>
    /// </summary>
    public class ViewModelLocator
    {
        private static MainViewModel _main;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view models
            ////}
            ////else
            ////{
            ////    // Create run time view models
            ////}

            CreateMain();
            CreateReadArticle();
            CreateReadPost();
            CreateChannel();
            CreateApplicationSettings();
            CreateViewComments();
        }

        private static ReadArticleViewModel _readArticleViewModel;

        /// <summary>
        /// Gets the ReadArticle property.
        /// </summary>
        public static ReadArticleViewModel ReadArticleStatic
        {
            get
            {
                if (_readArticleViewModel == null)
                {
                    CreateReadArticle();
                }

                return _readArticleViewModel;
            }
        }

        /// <summary>
        /// Gets the ReadArticle property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ReadArticleViewModel ReadArticle
        {
            get
            {
                return ReadArticleStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the ReadArticle property.
        /// </summary>
        public static void ClearReadArticle()
        {
            if (_readArticleViewModel != null)
                _readArticleViewModel.Cleanup();
            _readArticleViewModel = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the ReadArticle property.
        /// </summary>
        public static void CreateReadArticle()
        {
            if (_readArticleViewModel == null)
            {
                _readArticleViewModel = new ReadArticleViewModel();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            ClearChannel();
            ClearViewComments();
            ClearReadPost();
            ClearReadArticle();
            ClearApplicationSettings();
            ClearMain();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        public static MainViewModel MainStatic
        {
            get
            {
                if (_main == null)
                {
                    CreateMain();
                }

                return _main;
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return MainStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the Main property.
        /// </summary>
        public static void ClearMain()
        {
            if (_main != null)
                _main.Cleanup();
            _main = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the Main property.
        /// </summary>
        public static void CreateMain()
        {
            if (_main == null)
            {
                _main = new MainViewModel();
            }
        }


        #region Channel
        private static ChannelViewModel _chvm;

        /// <summary>
        /// Gets the Channel property.
        /// </summary>
        public static ChannelViewModel ChannelStatic
        {
            get
            {
                if (_chvm == null)
                {
                    CreateChannel();
                }

                return _chvm;
            }
        }

        /// <summary>
        /// Gets the Channel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ChannelViewModel Channel
        {
            get
            {
                return ChannelStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the Channel property.
        /// </summary>
        public static void ClearChannel()
        {
            if (_chvm != null)
                _chvm.Cleanup();
            _chvm = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the Channel property.
        /// </summary>
        public static void CreateChannel()
        {
            if (_chvm == null)
            {
                _chvm = new ChannelViewModel();
            }
        }
        #endregion

        #region ApplicationSettings
        private static ApplicationSettingsViewModel _as;

        /// <summary>
        /// Gets the ApplicationSettings property.
        /// </summary>
        public static ApplicationSettingsViewModel ApplicationSettingsStatic
        {
            get
            {
                if (_chvm == null)
                {
                    CreateApplicationSettings();
                }

                return _as;
            }
        }

        /// <summary>
        /// Gets the ApplicationSettings property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ApplicationSettingsViewModel ApplicationSettings
        {
            get
            {
                return ApplicationSettingsStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the ApplicationSettings property.
        /// </summary>
        public static void ClearApplicationSettings()
        {
            if (_as != null)
                _as.Cleanup();
            _as = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the ApplicationSettings property.
        /// </summary>
        public static void CreateApplicationSettings()
        {
            if (_as == null)
            {
                _as = new ApplicationSettingsViewModel();
            }
        }
        #endregion

        #region ViewComments
        private static ViewCommentsViewModel _viewcomments = null;

        /// <summary>
        /// Gets the ViewComments property.
        /// </summary>
        public static ViewCommentsViewModel ViewCommentsStatic
        {
            get
            {
                if (_viewcomments == null)
                {
                    CreateViewComments();
                }

                return _viewcomments;
            }
        }

        /// <summary>
        /// Gets the ViewCommentsStatic property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ViewCommentsViewModel ViewComments
        {
            get
            {
                return _viewcomments;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the ViewComments property.
        /// </summary>
        public static void ClearViewComments()
        {
            if (_viewcomments != null)
                _viewcomments.Cleanup();
            _viewcomments = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the ViewComments property.
        /// </summary>
        public static void CreateViewComments()
        {
            if (_viewcomments == null)
            {
                _viewcomments = new ViewCommentsViewModel();
            }
        }
        #endregion

        #region read post
        private static ReadPostViewModel _readPostViewModel;

        /// <summary>
        /// Gets the ReadPost property.
        /// </summary>
        public static ReadPostViewModel ReadPostStatic
        {
            get
            {
                if (_readPostViewModel == null)
                {
                    CreateReadPost();
                }

                return _readPostViewModel;
            }
        }

        /// <summary>
        /// Gets the ReadPost property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ReadPostViewModel ReadPost
        {
            get
            {
                return ReadPostStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the ReadPost property.
        /// </summary>
        public static void ClearReadPost()
        {
            if (_readPostViewModel != null)
                _readPostViewModel.Cleanup();
            _readPostViewModel = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the ReadPost property.
        /// </summary>
        public static void CreateReadPost()
        {
            if (_readPostViewModel == null)
            {
                _readPostViewModel = new ReadPostViewModel();
            }
        }

        #endregion
    }
}