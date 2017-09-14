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
        private readonly Func<KeyValuePair<string, StringValues>, bool> _whitelist;
        private readonly RequestDelegate _next;

        public HeaderPassthroughInboundMiddleware(RequestDelegate next, List<string> whitelist): this(next, pair => whitelist.Contains(pair.Key)) {}
        
        public HeaderPassthroughInboundMiddleware(RequestDelegate next, Func<KeyValuePair<string,StringValues>,bool> whitelist)
        {
            _next = next;
            _whitelist = whitelist;
        }

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers.Where(_whitelist);
            var outbound = new HeaderPassthroughOutboundMiddleware(headers);
            var cm = new HttpClientMiddleware();
            
            using (cm.Push(outbound))
            {
                await _next.Invoke(context);                
            }
        }
    }
}
