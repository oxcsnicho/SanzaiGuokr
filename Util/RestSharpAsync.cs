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
using RestSharp;
using System.Threading.Tasks;

namespace SanzaiGuokr.Util
{
    public class RestSharpAsync
    {
        public static Task<IRestResponse<T>> RestSharpExecuteAsyncTask<T>(RestClient c, RestRequest req) where T : new()
        {
            var t = new TaskCompletionSource<IRestResponse<T>>();

            TaskEx.Run(()=>c.ExecuteAsync<T>(req, s => t.TrySetResult(s)));

            return t.Task;
        }
        public static Task<IRestResponse> RestSharpExecuteAsyncTask(RestClient c, RestRequest req)
        {
            var t = new TaskCompletionSource<IRestResponse>();

            TaskEx.Run(()=>c.ExecuteAsync(req, s => t.TrySetResult(s)));

            return t.Task;
        }

    }
}
