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
    public class RestSharpAsync<T>
        where T:new()
    {
        public static Task<RestResponse<T>> RestSharpExecuteAsyncTask(RestClient c, RestRequest req)
        {
            var t = new TaskCompletionSource<RestResponse<T>>();

            c.ExecuteAsync<T>(req, s => t.TrySetResult(s));

            return t.Task;
        }

    }
}
