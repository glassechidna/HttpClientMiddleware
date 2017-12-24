# HttpClientMiddleware.MessageHandlerWrapperMiddleware

You might already have a library of useful `DelegatingHandler` implementations
that you don't necessarily want to rewrite to match the `IMiddleware` interface.
In that case, you can include them in your pipeline using this package.

## Usage

```csharp
using HttpClientMiddleware.MessageHandlerWrapperMiddleware;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        var wrapper = new MessageHandlerWrapperMiddleware(innner => new YourDelegatingHandler(inner));
        new HttpClientMiddlewareHandler().Register(wrapper);
    }
}
```

The `inner` parameter to the lambda can be ignored if you want to use a
non-delegating HttpMessageHandler. This is supported, but do note that any
middlewares pushed onto the pipeline _after_ your handler will be ignored.
This is fine if you are using the great [`RichardSzalay.MockHttp`][mock]
package for stubbed responses during tests.

[mock]: https://github.com/richardszalay/mockhttp
