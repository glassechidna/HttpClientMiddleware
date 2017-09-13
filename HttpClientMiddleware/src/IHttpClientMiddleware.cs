using System;
using System.Net.Http;

namespace HttpClientMiddleware
{
    public interface IHttpClientMiddleware
    {
        HttpMessageHandler GetHandler();
        void Register(IMiddleware middleware);
        IDisposable Push(params IMiddleware[] middlewares);
    }
}
