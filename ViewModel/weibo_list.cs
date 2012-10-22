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

namespace SanzaiGuokr.ViewModel
{
    public class weibo_list : object_list_base<status, WeiboResponse>
    {
        public weibo_list()
        {
            restClient.BaseUrl = "https://api.weibo.com";
        }

        protected override bool LoadMoreArticlesCanExecute()
        {
            return ViewModelLocator.ApplicationSettingsStatic.WeiboAccountAccessToken != "" // access_token should be valid
                && ArticleList.Count == 0; // forbid loading for more than one time
        }
        
        protected override RestRequest CreateRestRequest()
        {
            var req = new RestRequest();
            req.Resource = "2/statuses/user_timeline.json";
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
            req.Parameters.Add(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountAccessToken, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "screen_name", Value = "果壳网", Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 30, Type = ParameterType.GetOrPost });
            //req.Parameters.Add(new Parameter() { Name = "trim_user", Value = 1, Type = ParameterType.GetOrPost });
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
        public int previous_cursor { get; set; }
        public int next_cursor { get; set; }
        public int total_number { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
