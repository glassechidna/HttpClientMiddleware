using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMiddleware
{
    public interface IMiddleware
    {
        Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next);
    }
}
