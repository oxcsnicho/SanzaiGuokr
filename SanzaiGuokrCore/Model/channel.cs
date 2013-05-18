using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SanzaiGuokr.Model
{
    public class channel
    {
        private string _key;

        public string key
        {
            get { return _key; }
            set { _key = value; }
        }


        private string _intro;
        public string introduction
        {
            get { return _intro; }
            set { _intro = value; }
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _order;
        public int order
        {
            get { return _order; }
            set { _order = value; }
        }

        public string pic_large
        {
            get
            {
                return "/SanzaiGuokrMinisiteLogo;component/Resources/" + key + "_b.jpg";
            }
        }

        public string pic_small
        {
            get
            {
                return "/SanzaiGuokrMinisiteLogo;component/Resources/" + key + "_s.jpg";
            }
        }


        private RelayCommand _view;

        public RelayCommand ViewChannel
        {
            get
            {
                if (_view == null)
                {
                    _view = new RelayCommand(() => {
                        Messenger.Default.Send<channel>(this);
                    });
                }
                return _view;
            }

        }

        private minisite_article_list _al;
        public minisite_article_list MinisiteArticles
        {
            get
            {
                if (_al == null)
                    _al = new minisite_article_list(key) { Name = name };
                return _al;
            }
            set { _al = value; }
        }





    }
}
