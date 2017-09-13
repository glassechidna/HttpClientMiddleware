using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace HttpClientMiddleware.HeaderPassthroughMiddleware
{
    public class HeaderPassthroughOutboundMiddleware: IMiddleware
    {
        private readonly IEnumerable<KeyValuePair<string, StringValues>> _whitelist;

        public HeaderPassthroughOutboundMiddleware(IEnumerable<KeyValuePair<string, StringValues>> whitelist)
        {
            _whitelist = whitelist;
        }

        public Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            foreach (var pair in _whitelist)
            {
                foreach (var value in pair.Value) request.Headers.Add(pair.Key, value);
            }

            return next(request);
        }
    }
}