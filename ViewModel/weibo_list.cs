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
using SanzaiGuokr.Model;
using RestSharp;
using WeiboApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp.Deserializers;
using SanzaiGuokr.SinaApiV2;

namespace SanzaiGuokr.ViewModel
{
    public class weibo_list : object_list_base<status, WeiboResponse>
    {
        public weibo_list()
        {
            restClient.BaseUrl = "https://api.weibo.com";
            restClient.UserAgent = "";
        }

        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count == 0; // forbid loading for more than one time
        }
        
        protected override RestRequest CreateRestRequest()
        {
            var req = new RestRequest();
            req.Resource = "2/statuses/home_timeline.json";
            req.Method = Method.GET;
            req.RequestFormat = DataFormat.Json;
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
            };
            return req;
        }
        protected override void AddRestParameters(RestSharp.RestRequest req)
        {
            req.Parameters.Add(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin.access_token, Type=ParameterType.GetOrPost });
            //req.Parameters.Add(new Parameter() { Name = "screen_name", Value = "果壳网", Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 30, Type = ParameterType.GetOrPost });
            //req.Parameters.Add(new Parameter() { Name = "trim_user", Value = 1, Type = ParameterType.GetOrPost });
        }

        protected override async System.Threading.Tasks.Task pre_load_more()
        {
            if (!ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin.IsValid)
            {
                var client = new WebClient();
                try
                {
                    string data = await client.DownloadStringTaskAsync("http://ccrma.stanford.edu/~darkowen/temp/temp");
                    await TaskEx.Run(() =>
                    {
                        var J = new JsonDeserializer();
                        var res = J.Deserialize<SinaLogin>(data);
                        ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin = res;
                        if (!res.IsValid)
                        {
                            //  not able to handle the case when there is error
                            throw new Exception();
                        }
                    });
                }
                catch (Exception e)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("不太对"));
                }
            }
            
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
