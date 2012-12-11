using System;
using System.Collections.Generic;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.ViewModel
{
    public class comment_list : object_list_base<comment,List<comment>>
    {
        public comment_list(article_base a)
        {
            if (a == null)
                throw new ArgumentNullException();
            the_article = a;
        }
        private article_base _ar;
        private string the_article_PropertyName = "the_article";
        public article_base the_article
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

        protected override async System.Threading.Tasks.Task<List<comment>> get_data()
        {
            return await GuokrApi.GetComments(the_article, ArticleList.Count);
        }

    }
}
