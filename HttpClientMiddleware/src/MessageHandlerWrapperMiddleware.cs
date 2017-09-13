using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMiddleware
{
    public class MessageHandlerWrapperMiddleware : IMiddleware
    {
        private readonly Func<HttpMessageHandler, HttpMessageHandler> _handlerProvider;

        public MessageHandlerWrapperMiddleware(HttpMessageHandler handler): this(messageHandler => handler) {}
        
        public MessageHandlerWrapperMiddleware(Func<HttpMessageHandler, HttpMessageHandler> handlerProvider)
        {
            _handlerProvider = handlerProvider;
        }

        public Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            var inner = new DelegatingHandlerWrapperMiddleware(next);
            var handler = _handlerProvider(inner);
            var invoker = new HttpMessageInvoker(handler);
            return invoker.SendAsync(request, new CancellationToken());
        }
    }
    
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