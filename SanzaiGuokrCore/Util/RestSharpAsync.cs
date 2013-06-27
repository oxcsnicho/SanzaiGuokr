using System.Threading.Tasks;
using RestSharp;

namespace SanzaiGuokr.Util
{
    public class RestSharpAsync
    {
        public static Task<IRestResponse<T>> RestSharpExecuteAsyncTask<T>(RestClient c, RestRequest req) where T : new()
        {
            var t = new TaskCompletionSource<IRestResponse<T>>();

#if WP8
            Task.Run(() => c.ExecuteAsync<T>(req, s => t.TrySetResult(s)));
#else
            TaskEx.Run(()=>c.ExecuteAsync<T>(req, s => t.TrySetResult(s)));
#endif

            return t.Task;
        }
        public static Task<IRestResponse> RestSharpExecuteAsyncTask(RestClient c, RestRequest req)
        {
            var t = new TaskCompletionSource<IRestResponse>();

#if WP8
            Task.Run(() => c.ExecuteAsync(req, s => t.TrySetResult(s)));
#else
            TaskEx.Run(()=>c.ExecuteAsync(req, s => t.TrySetResult(s)));
#endif

            return t.Task;
        }

    }
}
