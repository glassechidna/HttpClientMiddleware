using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMiddleware
{
    public interface IHttpClientMiddlewareHandler
    {
        HttpMessageHandler Handler();
        IDisposable Push(params IMiddleware[] middlewares);
        Func<HttpRequestMessage, Task<HttpResponseMessage>> ComposedMiddleware(Func<HttpRequestMessage, Task<HttpResponseMessage>> baseFunc);
    }
}
