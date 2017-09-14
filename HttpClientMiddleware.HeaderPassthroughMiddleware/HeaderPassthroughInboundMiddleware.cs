using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HttpClientMiddleware.HeaderPassthroughMiddleware
{
    public class HeaderPassthroughInboundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HeaderPassthroughOptions _options;

        public HeaderPassthroughInboundMiddleware(RequestDelegate next, ICollection<string> whitelist): this(next, new HeaderPassthroughOptions
        {
            Whitelist = pair => whitelist.Contains(pair.Key)
        }) {}
        
        public HeaderPassthroughInboundMiddleware(RequestDelegate next, HeaderPassthroughOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers.Where(_options.Whitelist);
            var outbound = new HeaderPassthroughOutboundMiddleware(headers);
            var cm = new HttpClientMiddleware();
            
            using (cm.Push(outbound))
            {
                await _next.Invoke(context);                
            }
        }

        public class HeaderPassthroughOptions
        {
            public Func<KeyValuePair<string, StringValues>, bool> Whitelist;
        }
    }
}
