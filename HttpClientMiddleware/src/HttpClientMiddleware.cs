using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace HttpClientMiddleware
{
    public class HttpClientMiddleware : IHttpClientMiddleware
    {
        public HttpMessageHandler GetHandler()
        {
            return new Middleware(this);
        }

        public void Register(IMiddleware middleware)
        {
            var stack = GetOrCreateMiddlewareStack();
            Middlewares = stack.Push(middleware);
        }

        public IDisposable Push(params IMiddleware[] middlewares)
        {
            if (middlewares == null) throw new ArgumentNullException(nameof(middlewares));

            var stack = GetOrCreateMiddlewareStack();
            var bookmark = new ContextStackBookmark(stack, this);

            stack = middlewares.Aggregate(stack, (current, handler) => current.Push(handler));
            Middlewares = stack;

            return bookmark;
        }
        
        private static readonly AsyncLocal<ImmutableStack<IMiddleware>> Data = new AsyncLocal<ImmutableStack<IMiddleware>>();

        internal ImmutableStack<IMiddleware> Middlewares
        {
            get => Data.Value;
            set => Data.Value = value;
        }

        internal ImmutableStack<IMiddleware> GetOrCreateMiddlewareStack()
        {
            var middlewares = Middlewares;
            if (middlewares != null) return middlewares;
            
            middlewares = ImmutableStack<IMiddleware>.Empty;
            Middlewares = middlewares;
            return middlewares;
        }

        private sealed class ContextStackBookmark : IDisposable
        {
            private readonly ImmutableStack<IMiddleware> _bookmark;
            private readonly HttpClientMiddleware _httpClientMiddleware;

            public ContextStackBookmark(ImmutableStack<IMiddleware> bookmark, HttpClientMiddleware clientMiddleware)
            {
                _httpClientMiddleware = clientMiddleware;
                _bookmark = bookmark;
            }

            public void Dispose()
            {
                _httpClientMiddleware.Middlewares = _bookmark;
            }
        }
    }
}
