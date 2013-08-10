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
        private StatusType _status = StatusType.NOTLOADED;
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
        private RelayCommand _rna;
        public RelayCommand ReadNewArticles
        {
            get
            {
                if (_rna == null)
                    _rna = new RelayCommand(() =>
                    {
#if WP8
                        Task.Run(() => load_more());
#else
                        TaskEx.Run(() => load_more());
#endif

                    }, ReadNewArticlesCanExecute);
                return _rna;
            }
        }
        protected virtual bool ReadNewArticlesCanExecute()
        {
            return ArticleList.Count == 0 && Status != StatusType.INPROGRESS && Status != StatusType.UNDERCONSTRUCTION;
        }

        private RelayCommand _lma;
        public RelayCommand LoadMoreArticles
        {
            get
            {
                if (_lma == null)
                    _lma = new RelayCommand(() =>
                    {
#if WP8
                        Task.Run(() => load_more());
#else
                        TaskEx.Run(() => load_more());
#endif
                    }, LoadMoreArticlesCanExecute);
                return _lma;
            }
        }
        protected virtual bool LoadMoreArticlesCanExecute()
        {
            return true;
        }

        protected T last_last = default(T);
        public async virtual Task load_more(bool is_refresh = false)
        {
            if (!is_refresh && LoadMoreArticlesCanExecute() == false)
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
            catch (GuokrException e)
            {
#if DEBUG
                MessageBox.Show(e.errmsg);
#endif
                if (e.errnum == GuokrErrorCode.UnderConstruction)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.UNDERCONSTRUCTION);
                    return;
                }
                else
                    throw;
            }
            catch (Exception e)
            {
#if DEBUG
                MessageBox.Show(e.Message);
#endif
                Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.FAILED);
                return;
            }

            if (Data == null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.FAILED);
                return;
            }
            if (Data.Count() == 0)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.ENDED);
                return;
            }

            if (is_refresh)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ArticleList.Clear();
                    });
            else if (ArticleList.Count > 0)
                last_last = ArticleList[ArticleList.Count - 1];
            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                if (load_more_item_filter(item))
                    continue;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ArticleList.Add(item);
                    });
#if WP8
                await Task.Delay(150);
#else
                await TaskEx.Delay(150);
#endif
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.SUCCESS);

            post_load_more();
        }

        protected virtual async Task<TCollection> get_data()
        {
            return default(TCollection);
        }

        protected virtual async Task pre_load_more() { return; }
        protected virtual async void post_load_more() { }
        protected virtual bool load_more_item_filter(T item) { return false; }
    }
}
