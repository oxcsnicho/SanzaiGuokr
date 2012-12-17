using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SanzaiGuokr.Model;
using System.Threading;
using System.ComponentModel;

namespace SanzaiGuokr.ViewModel
{
    public abstract class object_list_base<T, TCollection> : INotifyPropertyChanged
        where TCollection : List<T>, new()
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(name)));
            }
        }

        public const string StatusPropertyName = "Status";
        private StatusType _status = StatusType.SUCCESS;
        public StatusType Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (_status == value)
                {
                    return;
                }

                var oldValue = _status;
                _status = value;

                RaisePropertyChanged(StatusPropertyName);
            }
        }

        public const string ArticleListPropertyName = "ArticleList";
        private ObservableCollection<T> _al = null;
        public ObservableCollection<T> ArticleList
        {
            get
            {
                if (_al == null)
                    _al = new ObservableCollection<T>();

                return _al;
            }

            set
            {
                if (_al == value)
                {
                    return;
                }

                _al = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(ArticleListPropertyName);
            }
        }

        private RelayCommand _lma;
        public RelayCommand LoadMoreArticles
        {
            get
            {
                if (_lma == null)
                    _lma = new RelayCommand(() =>
                    {
                        load_more();
                    }, LoadMoreArticlesCanExecute);
                return _lma;
            }
        }
        protected virtual bool LoadMoreArticlesCanExecute()
        {
            return true;
        }

        public async virtual Task load_more()
        {
            if (LoadMoreArticlesCanExecute() == false)
                return;

            if (Status == StatusType.INPROGRESS)
                return;

            //Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.INPROGRESS);
            Status = StatusType.INPROGRESS;
            await pre_load_more();

            TCollection Data = default(TCollection);

            try
            {
                Data = await get_data();
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.FAILED);
                return;
            }

            if (Data.Count() == 0)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.ENDED);
                return;
            }

            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                if (load_more_item_filter(item))
                    continue;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ArticleList.Add(item);
                    });
                Thread.Sleep(100);
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.SUCCESS);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                post_load_more();
            });
        }

        protected virtual async Task<TCollection> get_data()
        {
            return default(TCollection);
        }

        protected virtual async Task pre_load_more() { return; }
        protected virtual void post_load_more() { }
        protected virtual bool load_more_item_filter(T item) { return false; }
    }
}
