using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientMiddleware.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HttpClientMiddleware.HeaderPassthroughMiddleware
{
    public class HeaderPassthroughMiddleware: IMiddleware
    {
        public class Options
        {
            public Func<KeyValuePair<string, StringValues>, bool> Whitelist;
        }
        
        private readonly IHttpContextAccessor _accessor;
        private readonly Options _options;

        public HeaderPassthroughMiddleware(IHttpContextAccessor accessor, Options options)
        {
            _accessor = accessor;
            _options = options;
        }

        public Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            var headers = _accessor.HttpContext.Request.Headers.Where(kvp => _options.Whitelist(kvp));
            
            foreach (var pair in headers)
            {
                foreach (var value in pair.Value) request.Headers.Add(pair.Key, value);
            }

            return next(request);
        }
    }
    
    public static class HttpClientMiddlewareExtensions
    {
        public static MiddlewareBuilder UseHeaderPassthrough(this MiddlewareBuilder builder, HeaderPassthroughMiddleware.Options options)
        {
            builder.Add<HeaderPassthroughMiddleware>(options);
            return builder;
        }
    }
}