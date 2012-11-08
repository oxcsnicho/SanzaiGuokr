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
using System.Collections.Generic;
using SanzaiGuokr.GuokrObjects;

namespace SanzaiGuokr.ViewModel
{
    public class comment_list : object_list_base<comment,List<comment>>
    {
        public comment_list(article a)
        {
            req_resource = "api/reply/list/";

            if (a == null)
                throw new ArgumentNullException();
            the_article = a;
        }
        private article _ar;
        private string the_article_PropertyName = "the_article";
        public article the_article
        {
            get
            {
                return _ar;
            }
            private set
            {
                if (_ar == value)
                    return;
                _ar = value;
                RaisePropertyChanged(the_article_PropertyName);
            }
        }

        protected override RestRequest CreateRestRequest()
        {
            var req = base.CreateRestRequest();
            req.Method = Method.GET;
            return req;
        }
        protected override void AddRestParameters(RestSharp.RestRequest req)
        {
            req.Parameters.Add(new Parameter() { Name = "obj_id", Value = the_article.id, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "obj_type", Value = "article", Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 10, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = ArticleList.Count, Type = ParameterType.GetOrPost });
        }

    }
}
