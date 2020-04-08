using System;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAsyncProxyRequestService
    {
        Task<object> CreateRequest(string requestId);

        void RemoveRequest(string requestId);

        void SetRequestResponse(string requestId, object result);

        void SetRequestResponse(string requestId, Exception exception);

        void SetReququestCanceled(string requestId);
    }
}
