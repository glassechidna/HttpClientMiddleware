using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMiddleware
{
    public interface IHttpClientMiddlewareHandler
    {
        void Register(IMiddleware middleware);
        IDisposable Push(params IMiddleware[] middlewares);
        Func<HttpRequestMessage, Task<HttpResponseMessage>> ComposedMiddleware(Func<HttpRequestMessage, Task<HttpResponseMessage>> baseFunc);
    }
}
