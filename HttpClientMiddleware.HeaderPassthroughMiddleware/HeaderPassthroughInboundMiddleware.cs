using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HttpClientMiddleware.HeaderPassthroughMiddleware
{
    public class HeaderPassthroughInboundMiddleware
    {
        private readonly List<string> _whitelist;
        private readonly RequestDelegate _next;

        public HeaderPassthroughInboundMiddleware(RequestDelegate next, List<string> whitelist)
        {
            _next = next;
            _whitelist = whitelist;
        }

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers.Where(header => _whitelist.Contains(header.Key));
            var outbound = new HeaderPassthroughOutboundMiddleware(headers);
            var cm = new HttpClientMiddleware();
            
            using (cm.Push(outbound))
            {
                await _next.Invoke(context);                
            }
        }
    }
}
