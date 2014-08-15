using System.Threading.Tasks;
using RestSharp;
using System;
using System.Net;
using System.IO;

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

    public class WebClientAsync
    {
        public static Task<Stream> OpenReadAsync(Uri uri, String referer = null)
        {
            var tcs = new TaskCompletionSource<Stream>();

            var client = new WebClient();
            if (!String.IsNullOrEmpty(referer))
                client.Headers["Referer"] = referer;
            client.OpenReadCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    tcs.SetResult(e.Result);
                }
                else
                {
                    tcs.SetException(e.Error);
                }
            };

            client.OpenReadAsync(uri);

            return tcs.Task;
        }
        public static Task<string> DownloadStringAsync(Uri uri, String referer = null)
        {
            var tcs = new TaskCompletionSource<string>();

            var client = new WebClient();
            if (!String.IsNullOrEmpty(referer))
                client.Headers["Referer"] = referer;
            client.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    tcs.SetResult(e.Result);
                }
                else
                {
                    tcs.SetException(e.Error);
                }
            };

            client.DownloadStringAsync(uri);

            return tcs.Task;
        }
    }
}
