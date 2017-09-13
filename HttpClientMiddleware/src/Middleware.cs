using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMiddleware
{
    internal class Middleware : DelegatingHandler
    {
        private readonly HttpClientMiddleware _httpClientMiddleware;
        
        public Middleware(HttpClientMiddleware clientMiddleware): base(new HttpClientHandler())
        {
            _httpClientMiddleware = clientMiddleware;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stack = _httpClientMiddleware.GetOrCreateMiddlewareStack();
            
            var func = stack.Aggregate<IMiddleware, Func<HttpRequestMessage, Task<HttpResponseMessage>>>(
                req => base.SendAsync(request, cancellationToken), 
                (fn, middleware) => req => middleware.Invoke(req, fn)
            );
            
            return await func(request);
        }
    }
}
