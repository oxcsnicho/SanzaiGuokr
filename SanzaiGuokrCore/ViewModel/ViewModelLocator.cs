namespace SanzaiGuokr.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            CreateMain();
            CreateReadArticle();
            CreateReadPost();
            CreateChannel();
            CreateApplicationSettings();
            CreateViewComments();
            CreateBookmark();
        }

        private static ReadArticleViewModel _readArticleViewModel;
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
        public ReadArticleViewModel ReadArticle
        {
            get
            {
                return ReadArticleStatic;
            }
        }
        public static void ClearReadArticle()
        {
            if (_readArticleViewModel != null)
                _readArticleViewModel.Cleanup();
            _readArticleViewModel = null;
        }

        public static void CreateReadArticle()
        {
            if (_readArticleViewModel == null)
            {
                _readArticleViewModel = new ReadArticleViewModel();
            }
        }

        public static void Cleanup()
        {
            ClearChannel();
            ClearViewComments();
            ClearReadPost();
            ClearReadArticle();
            ClearApplicationSettings();
            ClearMain();
            ClearBookmark();
        }

        private static MainViewModel _main;
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
        public MainViewModel Main
        {
            get
            {
                return MainStatic;
            }
        }
        public static void ClearMain()
        {
            if (_main != null)
                _main.Cleanup();
            _main = null;
        }
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

        #region search article
        private static SearchArticleViewModel _searchArticleViewModel;

        /// <summary>
        /// Gets the ReadPost property.
        /// </summary>
        public static SearchArticleViewModel SearchArticleStatic
        {
            get
            {
                if (_searchArticleViewModel == null)
                {
                    CreateSearchArticle();
                }

                return _searchArticleViewModel;
            }
        }

        public SearchArticleViewModel SearchArticle
        {
            get
            {
                return SearchArticleStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the ReadPost property.
        /// </summary>
        public static void ClearSearchArticle()
        {
            if (_searchArticleViewModel != null)
                _searchArticleViewModel.Cleanup();
            _searchArticleViewModel = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the ReadPost property.
        /// </summary>
        public static void CreateSearchArticle()
        {
            if (_searchArticleViewModel == null)
            {
                _searchArticleViewModel = new SearchArticleViewModel();
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

        #region bookmark
        private static BookmarkViewModel _bookmarkViewModel;

        /// <summary>
        /// Gets the bookmark property.
        /// </summary>
        public static BookmarkViewModel BookmarkStatic
        {
            get
            {
                if (_bookmarkViewModel == null)
                {
                    CreateBookmark();
                }

                return _bookmarkViewModel;
            }
        }

        /// <summary>
        /// Gets the bookmark property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public BookmarkViewModel Bookmark
        {
            get
            {
                return BookmarkStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the bookmark property.
        /// </summary>
        public static void ClearBookmark()
        {
            if (_bookmarkViewModel != null)
                _bookmarkViewModel.Cleanup();
            _bookmarkViewModel = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the bookmark property.
        /// </summary>
        public static void CreateBookmark()
        {
            if (_bookmarkViewModel == null)
            {
                _bookmarkViewModel = new BookmarkViewModel();
            }
        }

        #endregion
    }
}