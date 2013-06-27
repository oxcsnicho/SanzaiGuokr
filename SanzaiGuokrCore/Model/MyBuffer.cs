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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanzaiGuokr.Model
{
    public enum BufferStatus
    {
        NotAvailable,
        InProgress,
        Completed,
        Failed
    };
    public class MyBuffer<T, U> where U : new()
    {
        class CompoundU
        {
            public BufferStatus Status { get; set; }
            public List<U> Inner { get; set; }
            public List<Action<List<U>>> Callback { get; set; }
        }

        Dictionary<T, CompoundU> Dict { get; set; }

        public MyBuffer()
        {
            Dict = new Dictionary<T, CompoundU>();
        }

        internal bool ContainsKey(T path)
        {
            return Dict.ContainsKey(path);
        }

        internal BufferStatus GetStatus(T path)
        {
            if (Dict.ContainsKey(path) && Dict[path] != null)
                return Dict[path].Status;
            else
                return BufferStatus.NotAvailable;
        }

        internal Task<List<U>> RetrieveBuf(T path)
        {
            var t = new TaskCompletionSource<List<U>>();

#if WP8
            Task.Run(() => RetrieveBufCallback(path, s => t.TrySetResult(s)));
#else
            TaskEx.Run(() => RetrieveBufCallback(path, s => t.TrySetResult(s)));
#endif

            return t.Task;
        }

        internal void RetrieveBufCallback(T path, Action<List<U>> callback)
        {
            if (Dict.ContainsKey(path) && Dict[path] != null)
            {
                if (Dict[path].Callback != null)
                    Dict[path].Callback.Add(callback);
                switch (Dict[path].Status)
                {
                    case BufferStatus.NotAvailable:
                    case BufferStatus.InProgress:
                    case BufferStatus.Failed:
                        // wait in queue
                        break;
                    case BufferStatus.Completed:
                        ExecuteCallback(path);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        void ExecuteCallback(T path)
        {
            if (Dict.ContainsKey(path) && Dict[path] != null && Dict[path].Inner != null && Dict[path].Callback != null)
            {
                foreach (var item in Dict[path].Callback)
                {
                    item.Invoke(Dict[path].Inner);
                }
                Dict[path].Callback.Clear();
            }
        }

        internal void SetBufToInProgress(T path)
        {
            if (!Dict.ContainsKey(path))
                Dict[path] = new CompoundU();
            if (Dict[path].Inner == null)
                Dict[path].Inner = new List<U>();
            if (Dict[path].Callback == null)
                Dict[path].Callback = new List<Action<List<U>>>();

            Dict[path].Status = BufferStatus.InProgress;
        }

        internal void PutBuffer(T path, List<U> list)
        {
            if (Dict.ContainsKey(path) && Dict[path] != null && Dict[path].Inner != null)
            {
                Dict[path].Inner = list;
                Dict[path].Status = BufferStatus.Completed;
                if (Dict[path].Callback != null)
                    ExecuteCallback(path);
            }
        }

        internal int GetBufLength(T path)
        {
            if (Dict.ContainsKey(path) && Dict[path] != null && Dict[path].Inner != null)
                return Dict[path].Inner.Count;
            else
                return 0;
        }

        internal void RefreshBuf(T path)
        {
            if (Dict.ContainsKey(path) && Dict[path] != null)
                Dict[path].Status = BufferStatus.NotAvailable;
        }

        internal async Task<List<U>> SafeGetBufRange(T path, int offset, int limit)
        {
            if (offset < 0 || limit < 0)
                throw new ArgumentNullException();

            var r = await RetrieveBuf(path);
            if (offset >= r.Count)
                return new List<U>();
            else if (offset + limit >= r.Count)
                limit = r.Count - offset;

            return r.GetRange(offset, limit);
        }
    }
}
