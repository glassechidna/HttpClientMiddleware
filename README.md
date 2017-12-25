# HttpClientMiddleware

[![Build Status](https://travis-ci.org/glassechidna/HttpClientMiddleware.svg?branch=master)](https://travis-ci.org/glassechidna/HttpClientMiddleware)
[![NuGet](https://img.shields.io/nuget/v/HttpClientMiddleware.svg)]()

## What

ASP.Net Core and OWIN provide a great pattern for constructing middlewares
that form part of a pipeline for inbound HTTP requests. HttpClientMiddleware
takes this pattern and applies it to outbound requests initiated by the
HttpClient class.

## Why

DelegatingHandler is a HttpMessageHandler implementation that can be passed
into the HttpClient constructor to wrap requests/responses in what is 
effectively a single middleware. You can manually compose these handlers,
but it can get fiddly and isn't nearly as easy the middleware pipelining
for inbound requests.

HttpClientMiddleware instead lets you register middlewares at application
launch (just like ASP.Net Core) on a `HttpClientMiddlewareHandler`, which
can then be used as a HttpMessageHandler to use in your HttpClient constructor. 

Middlewares can also be pushed onto the pipeline stack for shorter periods, 
e.g. if you want to do perform additional pipeline steps only for certain
transactions.

## Where

HttpClientMiddleware is available on [Nuget](https://www.nuget.org/packages/HttpClientMiddleware/).

## How

Usage is very straightforward. Anywhere you construct a HttpClient, do
the following:

```csharp
/* in all but the smallest toy programs, this is either a global or 
DI-injected. Suggested usage is as a DI-injected singleton with
middlewares registered at app startup, like ASP.Net Core.
*/
var middlewareHandler = new HttpClientMiddlewareHandler();
var client = new HttpClient(middlewareHandler);
```

Now all requests initiated using this HttpClient instance will pass through 
the middleware pipeline registered with this handler.

Part two is adding middlewares to the pipeline. This can be done in one of two
ways. The first is similar to ASP.Net Core inbound middlewares:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        /*
        this indirection is so that everything works with the right middleware
        handler when unit tests inject their own handler. see further down
        in the README for more details.
        */
        services.TryAddSingleton(new HttpClientMiddlewareHandler());
        var handler = services.BuildServiceProvider().GetService<HttpClientMiddlewareHandler>();
        handler.Register(new HostnameLoggerMiddleware());

        /* 
        consider using an injected HttpClient as below. then the rest of
        your app needn't know about IHttpClientMiddlewareHandler _and_ it will
        all use the registered pipelines automatically.
        */
        services.TryAddSingleton(sp => new HttpClient(handler));
    }
}

```

Part three is writing middlewares. This is only necessary if you want to do
something that the existing middlewares doesn't already do for you. You can
write a middleware as follows:

```csharp
public class HostnameLoggerMiddleware : IMiddleware
{
    public async Task<HttpResponseMessage> Invoke(HttpRequestMessage request, Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
    {
        var host = request.RequestUri.Host;
        Console.WriteLine($"req for {host}");
        var resp = await next(request);
        Console.WriteLine($"resp for {host}");
        return resp;
    }
}
```

You have the power to modify the supplied HttpRequest object in whichever way
you want, or even replace it with an entirely new request object. The same goes
for the response object. Just remember to call the provided `next()` to invoke
the next step in the pipeline.

### Advanced

If you want to push middlewares onto the pipeline for only a defined period, you
can do that too. It is done like so:

```csharp
HttpClient client = ...; // injected from somewhere hopefully

// ...

using (middlewareHandler.Push(new HostnameLoggerMiddleware()))
{
    var logged = await client.GetAsync("https://example.com"); // this one gets logged
}

var unlogged = await client.GetAsync("https://example.com"); // this one doesn't
```


### How can I use this to super-charge my tests?

So you've got a modern ASP.Net Core application: you'd probably describe it as
a microservice - it receives inbound HTTP requests and has to make outbound
HTTP requests in order to retrieve all the info it needs to formulate a response.
When writing tests for this kind of system, you have two options:

* Unit tests with mocks. Rather than making HTTP calls to your dependencies, you
  mock out some kind of "Service" class with expected response objects.
* Integration tests. Your tests send HTTP requests to real deployments of your
  dependencies and you hope that they're reliable.

HttpClientMiddleware provides a middleground when paired with [`RichardSzalay.MockHttp`][mock].
Your application can still make its calls to `HttpClient.GetAsync`, but stubbed
responses are provided by `MockHttpMessageHandler` and injected by `MockHttpMessageHandler`.
See an example here: 

[mock]: https://github.com/richardszalay/mockhttp

```csharp
    public class TestPassthrough
    {        
        [Fact]
        public async void TestE2E()
        {
            var mockHttp = new MockHttpMessageHandler();
            
            mockHttp.When("https://httpbin.org/headers")
                .Respond("application/json", @"{""headers"": {""Request-Id"": ""efgh-5678""}}");

            var server = new TestServer(new WebHostBuilder().ConfigureServices(services =>
            {
                services.TryAddSingleton(new HttpClientMiddlewareHandler(mockHttp));
            }).UseStartup<Startup>());

            var client = server.CreateClient();

            var resp = await client.GetAsync("http://api/");
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
            // during tests, there will already be one injected before this line. hence why we need
            // to use *Try*AddSingleton.
            services.TryAddSingleton(new HttpClientMiddlewareHandler());
            services.TryAddSingleton(sp => new HttpClient(sp.GetService<HttpClientMiddlewareHandler>()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async context =>
            {
                var client = context.RequestServices.GetService<HttpClient>();
                var resp = await client.GetAsync("https://httpbin.org/headers");
                var body = await resp.Content.ReadAsStringAsync();
                await context.Response.WriteAsync(body);
            });
        }
    }
```


## Provided middlewares

A few middlewares have been written to cover common use cases. Feel free to use
these or ignore them entirely. They are in separate Nuget packages, so you can
mix and match to your heart's content. The following links are to their respective
READMEs elsewhere in this repo.

* [HeaderPassthroughMiddleware](HttpClientMiddleware.HeaderPassthroughMiddleware/README.md)
* [MessageHandlerWrapperMiddleware](HttpClientMiddleware.MessageHandlerWrapperMiddleware/README.md)

## History

In HttpClientMiddleware 1.x, there was a `HttpClientMiddleware` class which used 
a `static` stack of middlewares, so "holding onto" that object was unnecessary. 
This was changed to the current `HttpClientMiddlewareHandler` class to allow for 
more advanced scenarios, like co-hosting two ASP.Net Core applications in a single 
process or registering (rather than pushing) middlewares in unit tests.

Another breaking change was removing `HttpClientMiddleware.GetHandler()` and 
making the `HttpClientMiddlewareHandler` class _itself_ the handler. This meant
that in simple cases the `HttpClient` would take care of maintaing the lifespan
of the middlewares.
