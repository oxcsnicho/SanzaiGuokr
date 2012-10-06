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

namespace SanzaiGuokr.ViewModel
{
    public class comment_list : object_list_base<comment>
    {
        public comment_list(article a)
        {
            req_resource = "api/reply/list/";

            if (a == null)
                throw new ArgumentNullException();
            Article = a;
        }
        article Article;

        protected override RestRequest CreateGuokrRestRequest()
        {
            var req = base.CreateGuokrRestRequest();
            req.Method = Method.GET;
            return req;
        }
        protected override void AddRestParameters(RestSharp.RestRequest req)
        {
            req.Parameters.Add(new Parameter() { Name = "obj_id", Value = Article.id, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "obj_type", Value = "article", Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 10, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = ArticleList.Count, Type = ParameterType.GetOrPost });
        }

    }
}
