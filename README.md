# HttpClientMiddleware

[![Build Status](https://travis-ci.org/glassechidna/HttpClientMiddleware.svg?branch=master)](https://travis-ci.org/glassechidna/HttpClientMiddleware)

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
launch (just like ASP.Net Core) and has a convenient accessor method that
can be used to retrieve a HttpMessageHandler to use in your HttpClient
constructor. 

Middlewares can also be pushed onto the pipeline stack for shorter periods, 
e.g. if you want to do perform additional pipeline steps only for certain
transactions.

## Where

HttpClientMiddleware is available on [Nuget](https://www.nuget.org/packages/HttpClientMiddleware/).

## How

Usage is very straightforward. Anywhere you construct a HttpClient, do
the following:

```csharp
/* feel free to DI inject this or instantiate a new one every time - the 
effect is the same. IHttpClientMiddleware and this public constructor 
are provided solely for mocking during unit tests. */
var client = new HttpClient(new HttpClientMiddleware().GetHandler());
```

Now all requests initiated using this HttpClient instance will pass through 
the middleware pipeline.

Part two is adding middlewares to the pipeline. This can be done in one of two
ways. The first is similar to ASP.Net Core inbound middlewares:

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        /* you don't need to hold onto the HttpClientMiddleware instance - but 
        feel free if you want to. we will define HostnameLoggerMiddleware in 
        the next step. */
        new HttpClientMiddleware().Register(new HostnameLoggerMiddleware());

        // the other normal stuff...
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
var client = new HttpClient(clientMiddleware.GetHandler());

// ...

using (new HttpClientMiddleware().Push(new HostnameLoggerMiddleware()))
{
    var logged = await client.GetAsync("https://example.com"); // this one gets logged
}

var unlogged = await client.GetAsync("https://example.com"); // this one doesn't
```

## Provided middlewares

A few middlewares have been written to cover common use cases. Feel free to use
these or ignore them entirely. They are in separate Nuget packages, so you can
mix and match to your heart's content. The following links are to their respective
READMEs elsewhere in this repo.

* [HeaderPassthroughMiddleware](HttpClientMiddleware.HeaderPassthroughMiddleware/README.md)
* [MessageHandlerWrapperMiddleware](HttpClientMiddleware.MessageHandlerWrapperMiddleware/README.md)
