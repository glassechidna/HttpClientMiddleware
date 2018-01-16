using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HttpClientMiddleware.AspNetCore
{
    public partial class HttpClientMiddlewareRegistrar
    {
        private readonly RequestDelegate _next;
        private readonly MiddlewareBuilder _builder;
        private readonly IHttpClientMiddlewareHandler _handler;

        public HttpClientMiddlewareRegistrar(RequestDelegate next, IHttpClientMiddlewareHandler handler,
            MiddlewareBuilder builder)
        {
            _next = next;
            _builder = builder;
            _handler = handler;
        }

        public async Task Invoke(HttpContext context)
        {
            var sp = context.Features.Get<IServiceProvidersFeature>().RequestServices;

            var middlewares = _builder.Definitions
                .Select(def => (IMiddleware) ActivatorUtilities.CreateInstance(sp, def.Type, def.Args)).ToArray();

            using (_handler.Push(middlewares))
            {
                await _next(context);
            }
        }
    }

    class MiddlewareBuilderDefinition
    {
        internal Type Type;
        internal object[] Args;

        public MiddlewareBuilderDefinition(Type type, object[] args)
        {
            Type = type;
            Args = args;
        }
    }

    public class MiddlewareBuilder
    {
        internal readonly List<MiddlewareBuilderDefinition> Definitions = new List<MiddlewareBuilderDefinition>();

        public void Add<TMiddleware>(params object[] args) where TMiddleware : IMiddleware
        {
            Definitions.Add(new MiddlewareBuilderDefinition(typeof(TMiddleware), args));
        }
    }

    public static partial class HttpClientMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpClientMiddleware(this IApplicationBuilder app,
            Action<MiddlewareBuilder> action)
        {
            var pb = new MiddlewareBuilder();
            action(pb);
            app.UseMiddleware<HttpClientMiddlewareRegistrar>(pb);
            return app;
        }

        public static IServiceCollection AddHttpClientMiddleware(this IServiceCollection services,
            HttpClientMiddlewareServiceOptions options = null)
        {
            if (options == null) options = new HttpClientMiddlewareServiceOptions();

            var handler = new HttpClientMiddlewareHandler(options.InnerHandler);
            services.TryAddSingleton<IHttpClientMiddlewareHandler>(handler);
            
            if (options.InjectHttpClient) services.TryAddSingleton(sp =>
            {
                var middleware = sp.GetService<IHttpClientMiddlewareHandler>();
                return new HttpClient(middleware.Handler());
            });

            return services;
        }
    }

    public class HttpClientMiddlewareServiceOptions
    {
        public bool InjectHttpClient = true;
        public HttpMessageHandler InnerHandler = new HttpClientHandler();
    }
}
