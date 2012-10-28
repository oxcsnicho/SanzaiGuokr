using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.Messages
{
    public enum ErrorType
    {
        NO_ERROR,
        ERROR_CANNOT_GO_PREVIOUS,
        ERROR_CANNOT_GO_NEXT
    };
    class GoToReadArticleComment
    {
        public article article { get; set; }
        public ErrorType error;
    }
    class GoToReadArticle
    {
        public article article { get; set; }
        public ErrorType error;
    }
    class ViewImageMessage
    {
        public string small_uri { get; set; }
        public string med_uri { get; set; }
        public string large_uri { get; set; }
    }
}
