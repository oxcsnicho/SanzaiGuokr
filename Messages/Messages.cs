using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanzaiGuokr.Model;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.GuokrObject;

namespace SanzaiGuokr.Messages
{
    class ChannelLoadFailureMessage
    {
    }
    public class ReferenceCommentMessage
    {
        public comment comment { get; set; }
    }
    public enum ErrorType
    {
        NO_ERROR,
        ERROR_CANNOT_GO_PREVIOUS,
        ERROR_CANNOT_GO_NEXT
    };
    class GoToReadArticleComment
    {
        public article_base article { get; set; }
        public ErrorType error;
    }
    class GoToReadArticle
    {
        public article article { get; set; }
        public ErrorType error;
    }
    class GoToReadPost
    {
        public GuokrPost article { get; set; }
        public ErrorType error;
    }
    class ViewImageMessage
    {
        public string small_uri { get; set; }
        public string med_uri { get; set; }
        public string large_uri { get; set; }
    }
    class DeleteCommentComplete
    {
        public GuokrException Exception { get; set; }
        public comment comment { get; set; }
    }
    class CreateAWeibo
    {
        public enum ShareWeiboType
        {
            Normal,
            ShareGuokrArticle
        };
        public ShareWeiboType Type { get; set; }
        public article Article { get; set; }
    }
    class ReposeAWeibo
    {
        public WeiboApi.status Status { get; set; }
    }
    class CommentAWeibo
    {
        public WeiboApi.status Status { get; set; }
    }
#if DEBUG
    class MyWebBrowserStatusChanged
    {
        public string NewStatus { get; set; }
    }
#endif
}
