using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMiddleware.AspNetCore
{
    delegate Task<HttpResponseMessage> SendAsyncDelegate(HttpRequestMessage request, CancellationToken cancellationToken);

    class DelegatingHandlerCallbacker : DelegatingHandler
    {
        private readonly SendAsyncDelegate _delegate;

        public DelegatingHandlerCallbacker(SendAsyncDelegate dele)
        {
            _delegate = dele;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _delegate(request, cancellationToken);
        }
    }
    
    class DelegatingWrapper : IMiddleware
    {
        private readonly SendAsyncDelegate _inner;
        private readonly AsyncLocal<Func<HttpRequestMessage, Task<HttpResponseMessage>>> _next;

        public DelegatingWrapper(DelegatingHandler handler)
        {
            _next = new AsyncLocal<Func<HttpRequestMessage, Task<HttpResponseMessage>>>();
            handler.InnerHandler = new DelegatingHandlerCallbacker(Callback);
            
            var dynMethod = handler.GetType().GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            _inner = (SendAsyncDelegate)dynMethod.CreateDelegate(typeof(SendAsyncDelegate), handler);
        }

        private Task<HttpResponseMessage> Callback(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _next.Value(request);
        }

        public Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            _next.Value = next;
            return _inner(request, CancellationToken.None);
        }
    }
}
