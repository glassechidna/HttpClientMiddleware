using System;
using System.Collections.Generic;
using System.Net.Http;
using HttpClientMiddleware.AspNetCore;
using HttpClientMiddleware.HeaderPassthroughMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace HttpClientMiddleware.Tests
{
    public class TestPassthrough
    {
        [Fact]
        public async void Test1()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            var client = server.CreateClient();

            var msg = new HttpRequestMessage { Method = HttpMethod.Get };
            msg.Headers.Add("Request-Id", "abcd-1234");
            msg.RequestUri = new Uri("http://api/");
            
            var resp = await client.SendAsync(msg);            
            var body = await resp.Content.ReadAsStringAsync();
            
            var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(body);
            var reqId = json["headers"]["Request-Id"];
            Assert.Equal(reqId, "abcd-1234");
        }
        
        [Fact]
        public async void TestMock()
        {
            var mockHttp = new MockHttpMessageHandler();
            
            mockHttp.When("https://httpbin.org/headers")
                .Respond("application/json", @"{""headers"": {""Request-Id"": ""efgh-5678""}}");

            var server = new TestServer(new WebHostBuilder().ConfigureServices(services =>
            {
                services.TryAddSingleton<IHttpClientMiddlewareHandler>(new HttpClientMiddlewareHandler(mockHttp));
            }).UseStartup<Startup>());

            var client = server.CreateClient();

            var msg = new HttpRequestMessage { Method = HttpMethod.Get };
            msg.Headers.Add("Request-Id", "abcd-1234");
            msg.RequestUri = new Uri("http://api/");
            
            var resp = await client.SendAsync(msg);            
            var body = await resp.Content.ReadAsStringAsync();
            
            var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(body);
            var reqId = json["headers"]["Request-Id"];
            Assert.Equal(reqId, "efgh-5678");
        }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClientMiddleware();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<HeaderPassthroughInboundMiddleware>(new List<string> {"Request-Id"});
            
            app.Run(async context =>
            {
                var client = context.RequestServices.GetService<HttpClient>();
                var resp = await client.GetAsync("https://httpbin.org/headers");
                var body = await resp.Content.ReadAsStringAsync();
                await context.Response.WriteAsync(body);
            });
        }
    }
}