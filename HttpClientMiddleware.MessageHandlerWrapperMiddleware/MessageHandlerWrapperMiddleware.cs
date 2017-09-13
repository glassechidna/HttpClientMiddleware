using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMiddleware.MessageHandlerWrapperMiddleware
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
}