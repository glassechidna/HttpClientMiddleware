using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMiddleware.MessageHandlerWrapperMiddleware
{
    internal class DelegatingHandlerWrapperMiddleware : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _next;

        public DelegatingHandlerWrapperMiddleware(Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            _next = next;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _next(request);
        }
    }
}