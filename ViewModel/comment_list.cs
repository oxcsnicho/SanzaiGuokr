using System;
using System.Collections.Generic;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Model;
using SanzaiGuokr.GuokrObject;

namespace SanzaiGuokr.ViewModel
{
    public class comment_list : object_list_base<comment,List<comment>>
    {
        public comment_list(GuokrObjectWithId a)
        {
            if (a == null)
                throw new ArgumentNullException();
            the_article = a;
        }
        private GuokrObjectWithId _ar;
        private string the_article_PropertyName = "the_article";
        public GuokrObjectWithId the_article
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
            return await GuokrApi.GetCommentsV2(the_article, ArticleList.Count);
        }

    }
}
