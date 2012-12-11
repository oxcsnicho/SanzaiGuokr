using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
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
    public class ChannelViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ChannelViewModel class.
        /// </summary>
        public ChannelViewModel()
        {
            if (IsInDesignMode)
            {
                the_channel = new channel()
                {
                    pic_large = "http://m.guokr.com/skin/mini_logo/fact_b.jpg",
                    introduction = "捍卫真相与细节，一切谣言将在这里被终结",
                    pic_small = "http://m.guokr.com/skin/mini_logo/fact_s.jpg",
                    id = 14,
                    order = 1,
                    name = "谣言粉碎机"
                };
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
            }

            Messenger.Default.Register<channel>(this, (action) =>
            {
                the_channel = action;
                if (the_channel.MinisiteArticles.ArticleList.Count == 0)
                    the_channel.MinisiteArticles.load_more();
            });
        }

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


        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}