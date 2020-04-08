using HES.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AsyncProxyRequestService : IAsyncProxyRequestService
    {
        ConcurrentDictionary<string, TaskCompletionSource<object>> _requests = new ConcurrentDictionary<string, TaskCompletionSource<object>>();

        /// <exception cref="ArgumentException">Throws if request with passed id is not found</exception>
        TaskCompletionSource<object> FindRequest(string requestId)
        {
            var requestFound = _requests.TryGetValue(requestId, out TaskCompletionSource<object> request);

            if (!requestFound)
                throw new ArgumentException("Request with passed id not found", nameof(requestId));

            return request;
        }

        public void RemoveRequest(string requestId)
        {
            _requests.TryRemove(requestId, out TaskCompletionSource<object> removedRequest);
        }

        /// <exception cref="ArgumentException">Throws if request with passed id already present</exception>
        public Task<object> CreateRequest(string requestId)
        {
            TaskCompletionSource<object> request = new TaskCompletionSource<object>();

            var addResult = _requests.TryAdd(requestId, request);

            if (!addResult)
                throw new ArgumentException("Request with passed id already present", nameof(requestId));

            return request.Task;
        }

        /// <exception cref="ArgumentException">Throws if request with passed id is not found</exception>
        public void SetRequestResponse(string requestId, object result)
        {
            FindRequest(requestId).TrySetResult(result);
        }

        /// <exception cref="ArgumentException">Throws if request with passed id is not found</exception>
        public void SetRequestResponse(string requestId, Exception exception)
        {
            FindRequest(requestId).TrySetException(exception);
        }

        /// <exception cref="ArgumentException">Throws if request with passed id is not found</exception>
        public void SetReququestCanceled(string requestId)
        {
            FindRequest(requestId).TrySetCanceled();
        }
    }
}
