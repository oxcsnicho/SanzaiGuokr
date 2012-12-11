using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SanzaiGuokr.Model
{
    public class channel
    {
        private int _id;
        public int id
        {
            get { return _id; }
            set { _id = value; }
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

        private string _pic_large;
        public string pic_large
        {
            get
            {
                if (_pic_large == null)
                    return null;
                var split = _pic_large.Split(new char[] { '/' });
                if (split != null && split.Length > 0)
                {
                    return "Resources/"+split[split.Length - 1];
                }
                else
                    return _pic_large;
            }
            set { _pic_large = value; }
        }

        private string _pic_small;
        public string pic_small
        {
            get
            {
                if (_pic_small == null)
                    return null;
                var split = _pic_small.Split(new char[] { '/' });
                if (split != null && split.Length > 0)
                {
                    return "Resources/"+split[split.Length - 1];
                }
                else
                    return _pic_small;
            }
            set { _pic_small = value; }
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
                    _al = new minisite_article_list(id) { Name = name };
                return _al;
            }
            set { _al = value; }
        }





    }
}
