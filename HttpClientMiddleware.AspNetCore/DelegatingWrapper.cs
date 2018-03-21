using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMiddleware.AspNetCore
{
    delegate Task<HttpResponseMessage> SendAsyncDelegate(HttpRequestMessage request, CancellationToken cancellationToken);

    class DelegatingWrapper : IMiddleware
    {
        private SendAsyncDelegate _inner;

        public DelegatingWrapper(DelegatingHandler handler)
        {
            MethodInfo dynMethod = handler.GetType().GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            _inner = (SendAsyncDelegate)dynMethod.CreateDelegate(typeof(SendAsyncDelegate), handler);
        }

        public Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            return _inner(request, CancellationToken.None);
        }
    }
}
