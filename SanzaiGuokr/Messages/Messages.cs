using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanzaiGuokr.Model;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.GuokrObject;

namespace SanzaiGuokr.Messages
{
#if false
    class ChannelLoadFailureMessage
    {
    }
#endif
    public class ReferenceCommentMessage
    {
        public comment comment { get; set; }
    }
#if false
    public enum ErrorType
    {
        NO_ERROR,
        ERROR_CANNOT_GO_PREVIOUS,
        ERROR_CANNOT_GO_NEXT
    };
#endif
    class GoToReadArticleComment
    {
        public article_base article { get; set; }
        // public ErrorType error;
    }
    class GoToReadArticle
    {
        public article article { get; set; }
        // public ErrorType error;
    }
    class GoToReadPost
    {
        public GuokrPost article { get; set; }
        // public ErrorType error;
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
#if false
#endif

    // TODO: though this message is not useful at this moment, it would be useful when we need to support sharing other GuokrObjects through weibo
    // keeping it as is and later we will do refactor
    // need to change WeiboCreate.xaml.cs to capture this message and reference the right object to share through weibo
    public class CreateAWeibo
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
