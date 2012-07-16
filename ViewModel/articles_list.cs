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
using System.Collections.ObjectModel;
using RestSharp;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.Messages;

namespace SanzaiGuokr.Model
{

    public enum StatusType
    {
        SUCCESS,
        INPROGRESS,
        FAILED
    }
    public class article_list : ViewModelBase
    {
        public article GetDefaultArticleOfThisList()
        {
            return DefaultArticle;
        }

        private article _da;
        public article DefaultArticle
        {
            get
            {
                if (_da == null)
                {
                    _da = new article();
                    _da.parent_list = this;
                }
                return _da;
            }
        }


        public article_list()
        {
            ArticleList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ArticleListCollectionChanged);
        }

        void ArticleListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (article item in e.NewItems)
                {
                    item.parent_list = this;
                }
        }
        public string Name { get; set; }
        /// <summary>
        /// The <see cref="ArticleList" /> property's name.
        /// </summary>
        public const string ArticleListPropertyName = "ArticleList";

        private ObservableCollection<article> _al = null;

        /// <summary>
        /// Gets the ArticleList property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<article> ArticleList
        {
            get
            {
                if (_al == null)
                    _al = new ObservableCollection<article>();

                return _al;
            }

            set
            {
                if (_al == value)
                {
                    return;
                }

                var oldValue = _al;
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
                    });
                return _lma;
            }
        }

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


        RestClient c = new RestClient("http://m.guokr.com");

        protected string req_resource = "api/content/latest_article_list/";
        protected virtual void prepare_parameter(RestRequest req)
        {
            req.Parameters.Add(new Parameter() { Name = "count", Value = 8, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = ArticleList.Count, Type = ParameterType.GetOrPost });
        }

        public void load_more()
        {
            if (Status == StatusType.INPROGRESS)
                return;

            var req = new RestRequest();
            req.Resource = req_resource;
            prepare_parameter(req);
            req.Method = Method.POST;
            req.RequestFormat = DataFormat.Json;
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
            };

            c.ExecuteAsync<List<article>>(req, (response) =>
            {
                if (response.Data == null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(()=>Status = StatusType.FAILED);
                    return;
                }

                article last_last = null;
                if (ArticleList.Count > 0)
                    last_last = ArticleList[ArticleList.Count - 1];

                for (int i = 0; i < response.Data.Count; i++)
                {
                    var item = response.Data[i];
                    if (item.minisite_name == "性 情"
                        && DateTime.Now < new DateTime(2012, 7, 20))
                        continue;

                    Deployment.Current.Dispatcher.BeginInvoke(() => 
                        {
                            ArticleList.Add(item);
                        });
                }
                Deployment.Current.Dispatcher.BeginInvoke(() => Status = StatusType.SUCCESS);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (last_last != null)
                        last_last.ReadNextArticle.RaiseCanExecuteChanged();
                });
            });
            Status = StatusType.INPROGRESS;
        }
    }

    public class minisite_article_list : article_list
    {
        public int minisite_id { get; set; }
        public minisite_article_list(int _id)
        {
            minisite_id = _id;
            req_resource = "api/content/minisite_article_list/";
        }

        protected override void prepare_parameter(RestRequest req)
        {
            req.AddParameter(new Parameter() { Name = "minisite_id", Value = minisite_id, Type = ParameterType.GetOrPost });
            base.prepare_parameter(req);
        }
    }
}
