using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using RestSharp;
using SanzaiGuokr.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using SanzaiGuokr.Util;

namespace SanzaiGuokr.ViewModel
{
    public abstract class object_list_base<T, TCollection> : ViewModelBase
        where TCollection : IEnumerable<T>, new()
    {

        #region status indicators
        /// <summary>
        /// The <see cref="Status" /> property's name.
        /// </summary>
        public const string StatusPropertyName = "Status";

        private StatusType _status = StatusType.SUCCESS;

        /// <summary>
        /// Gets the Status property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
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
        #endregion

        /// <summary>
        /// The <see cref="ArticleList" /> property's name.
        /// </summary>
        public const string ArticleListPropertyName = "ArticleList";

        private ObservableCollection<T> _al = null;

        /// <summary>
        /// Gets the ArticleList property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
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

        protected RestClient restClient = GuokrApi.Client;

        protected virtual async Task<TCollection> get_data()
        {
            var req = CreateRestRequest();
            AddRestParameters(req);
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<TCollection>(restClient, req);
            if (resp == null || resp.Data == null)
                throw new WebException();
            return resp.Data;
        }

        public async virtual Task load_more()
        {
            if (LoadMoreArticlesCanExecute() == false)
                return;

            if (Status == StatusType.INPROGRESS)
                return;

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

            for (var it = Data.GetEnumerator(); it.MoveNext(); )
            {
                var item = it.Current;
                if (load_more_item_filter(item))
                    continue;
                await TaskEx.Delay(100);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ArticleList.Add(item);
                    });
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.SUCCESS);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                post_load_more();
            });
        }

        protected virtual bool load_more_item_filter(T item) { return false; }
        protected abstract void AddRestParameters(RestRequest req);
        protected virtual RestRequest CreateRestRequest()
        {
            RestRequest req = new RestRequest();
            req.Resource = req_resource;
            req.Method = Method.POST;
            req.RequestFormat = DataFormat.Json;
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
            };
            return req;
        }
        protected virtual void post_load_more() { }
        protected string req_resource;
        protected virtual async Task pre_load_more() { return; }
    }
}
