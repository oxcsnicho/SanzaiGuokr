using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using GalaSoft.MvvmLight.Command;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class ReadArticleViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ReaArticleViewModel class.
        /// </summary>
        public ReadArticleViewModel()
        {
            if (IsInDesignMode)
            {
                the_article = ViewModelLocator.MainStatic.latest_articles[0];
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
            }

            Messenger.Default.Register<GoToReadArticle>(this, (action) =>
                {
                    the_article = action.article;
                });
        }

        #region the_channel
        /// <summary>
        /// The <see cref="the_channel" /> property's name.
        /// </summary>
        public const string the_channelPropertyName = "the_channel";

        private channel _ch = null;

        /// <summary>
        /// Gets the the_channel property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public channel the_channel
        {
            get
            {
                return _ch;
            }

            set
            {
                if (_ch == value)
                {
                    return;
                }

                var oldValue = _ch;
                _ch = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(the_channelPropertyName);
            }
        }
        #endregion

        #region the_article

        /// <summary>
        /// The <see cref="the_article" /> property's name.
        /// </summary>
        public const string the_articlePropertyName = "the_article";

        private article _art;
        /// <summary>
        /// Gets the the_article property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public article the_article
        {
            get
            {
                return _art;
            }

            set
            {
                if (_art == value)
                {
                    return;
                }

                var oldValue = _art;
                _art = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(the_articlePropertyName);
            }
        }

        #endregion

        #region progress indicator

        private RelayCommand _setli;
        public RelayCommand SetLoadingIndicator
        {
            get
            {
                if(_setli == null)
                    _setli = new RelayCommand(()=>
                {
                    LoadingIndicator = true;
                });

                return _setli;
            }
        }
        private RelayCommand _resetli;
        public RelayCommand ResetLoadingIndicator
        {
            get
            {
                if(_resetli == null)
                    _resetli = new RelayCommand(()=>
                {
                    LoadingIndicator = false;
                });

                return _resetli;
            }
        }

        /// <summary>
        /// The <see cref="LoadingIndicator" /> property's name.
        /// </summary>
        public const string LoadingIndicatorPropertyName = "LoadingIndicator";

        private bool _li = false;

        /// <summary>
        /// Gets the LoadingIndicator property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public bool LoadingIndicator
        {
            get
            {
                return _li;
            }

            set
            {
                if (_li == value)
                {
                    return;
                }

                var oldValue = _li;
                _li = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(LoadingIndicatorPropertyName);
            }
        }

        #endregion
        
    }
}