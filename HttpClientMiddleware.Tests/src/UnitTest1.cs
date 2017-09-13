using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace HttpClientMiddleware.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async void Test2()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("http://localhost/api/user/*")
                .Respond("application/json", "{'name' : 'Test McGee'}");
            
            var middleware = new HttpClientMiddleware();
            var client = new HttpClient(middleware.GetHandler());

            using (middleware.Push(
                new MessageHandlerWrapperMiddleware.MessageHandlerWrapperMiddleware(h => new Delegator(h)),
                new MessageHandlerWrapperMiddleware.MessageHandlerWrapperMiddleware(mockHttp)
            ))
            {
                var resp = await client.GetAsync("http://localhost/api/user/abc");
                var body = await resp.Content.ReadAsStringAsync();
                Assert.Equal(body, "{'name': 'Test McGee'}");
            }
            

        }
        
//        [Fact]
//        public async void Test1()
//        {
//            var middleware = new HttpClientMiddleware();
//            var client = new HttpClient(middleware.GetHandler());
//            middleware.Register(new HostnameMiddleware
//            {
//                Ident = "A"
//            });
//
//            middleware.Register(new HostnameMiddleware
//            {
//                Ident = "B"
//            });
//
//            using (middleware.Push(new TestMiddleware()))
//            {
//                var resp = await client.GetAsync("https://google.com");
//                var body = await resp.Content.ReadAsStringAsync();
//                Console.WriteLine($"length is {body.Length}");
//
//                await Task.Run(async () =>
//                {
//                    var resp3 = await client.GetAsync("https://bing.com");
//                    var body3 = await resp3.Content.ReadAsStringAsync();
//                    Console.WriteLine($"length is {body3.Length}");
//                });
//            }
//            
//            var resp2 = await client.GetAsync("https://example.com");
//            var body2 = await resp2.Content.ReadAsStringAsync();
//            Console.WriteLine($"length is {body2.Length}");
//        }
    }

    public class Delegator : DelegatingHandler
    {
        public Delegator(HttpMessageHandler innerHandler) : base(innerHandler) {}

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("before");
            var resp = await base.SendAsync(request, cancellationToken);
            Console.WriteLine("after");
            return resp;
        }
    }

    public class TestMiddleware : IMiddleware
    {
        public async Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            Console.WriteLine($"hey i got {request}");
            var resp = await next(request);
            Console.WriteLine($"now i got {resp.StatusCode} for it");
            return resp;
        }
    }
    
    public class HostnameMiddleware : IMiddleware
    {
        public string Ident;
        
        public async Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            var host = request.RequestUri.Host;
            Console.WriteLine($"req {Ident} for {host}");
            var resp = await next(request);
            Console.WriteLine($"resp {Ident} for {host}");
            return resp;
        }
    }
}