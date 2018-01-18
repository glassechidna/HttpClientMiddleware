using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMiddleware
{
    public class LambdaMiddleware: IMiddleware
    {
        private readonly Func<HttpRequestMessage, Func<HttpRequestMessage, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> _func;

        public LambdaMiddleware(Func<HttpRequestMessage, Func<HttpRequestMessage, Task<HttpResponseMessage>>,Task<HttpResponseMessage>> func)
        {
            _func = func;
        }

        public Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            return _func(request, next);
        }
    }
}
