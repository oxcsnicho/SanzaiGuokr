using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using RestSharp;
using SanzaiGuokr.Model;
using SanzaiGuokr.SinaApiV2;
using SanzaiGuokr.Util;
using WeiboApi;

namespace SanzaiGuokr.ViewModel
{
    public class weibo_list : object_list_base<status, List<status>>
    {
        protected RestClient restClient;
        public weibo_list()
        {
            restClient = new RestClient();
            restClient.BaseUrl = "https://api.weibo.com";
            restClient.UserAgent = "";
        }
        protected override async void post_load_more()
        {
            var mytopstatus = await SinaApiV2.SinaApiV2.MyFirstPost();
            Deployment.Current.Dispatcher.BeginInvoke(() => ArticleList.Insert(0, mytopstatus));
        }

        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count == 0; // forbid loading for more than 100 items
        }

        bool has_refreshed_token = false;
        protected override async System.Threading.Tasks.Task pre_load_more()
        {
            if (!ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin.IsValid)
            {
                var client = new RestClient(SinaApiConfig.StanfordLocationBaseUrl);
                var req = new RestRequest();
                req.Resource = SinaApiConfig.StanfordLocationResource;
                req.Method = Method.GET;
                req.RequestFormat = DataFormat.Json;
                req.OnBeforeDeserialization = (resp) => { resp.ContentType = "application/json"; };
                try
                {
                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<SinaLogin>(client, req);
                    var res = response.Data;
                    if (res == null)
                        throw new WebException();
                    ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin = res;
                    if (!res.IsValid)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("果壳君微博load不出来了，请联系作者君或坐等他修bug\n\n （￣Ｑ￣）╯"));
                    }
                    has_refreshed_token = true;
                }
                catch
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("果壳君微博load不出来了，请联系作者君或坐等他修bug\n\n （￣Ｑ￣）╯"));
                }
            }

        }

        protected override async Task<List<WeiboApi.status>> get_data()
        {
            List<status> data = null;
            try
            {
                data = await SinaApiV2.SinaApiV2.MrGuokrHomeTimeline();
            }
            catch (SinaWeiboException e)
            {
                if (e.error == "invalid_access_token" || e.error_code == 21321 || e.error_code == 21319
                    || e.error_code == 21317 || e.error_code == 21316 || e.error_code == 21315 || e.error_code == 21314)
                {
                    if (!has_refreshed_token)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin = new SinaLogin();
                                Status = StatusType.FAILED;
                                load_more();
                            });
                    }
                }
                throw e;
            }
            return data;
        }

        protected override bool load_more_item_filter(status item)
        {
            item.normalize();
            if (item.user != null)
                item.user.normalize();
            if (item.retweeted_status != null)
                item.retweeted_status.normalize();
            return false;
        }

    }
    public class WeiboResponse : IEnumerable<status>
    {
        public WeiboResponse()
        {
            Statuses = new List<status>();
        }
        private List<status> _statuses;

        public List<status> Statuses
        {
            get { return _statuses; }
            set { _statuses = value; }
        }

        public IEnumerator<status> GetEnumerator()
        {
            return Statuses.GetEnumerator();
        }

        public bool hasVisible { get; set; }
        public Int64 previous_cursor { get; set; }
        public Int64 next_cursor { get; set; }
        public Int64 total_number { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
}
