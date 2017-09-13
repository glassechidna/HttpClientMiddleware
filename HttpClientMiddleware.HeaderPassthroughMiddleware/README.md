# HttpClientMiddleware.HeaderPassthroughMiddleware

In a microservice world, you often want to pass request IDs received by a 
service onto downstream services. This can either be done by stuffing
request IDs into HttpContexts and exposing that everywhere that needs it,
or it can be done using HttpClient middleware.

## Usage

```csharp
using HttpClientMiddleware.HeaderPassthroughMiddleware;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<HeaderPassthroughInboundMiddleware>(
            new List<string>{"X-Request-Id", "X-Correlation-Id"}
        );
        
        // alternatively, if you need more flexibility:
        app.UseMiddleware<HeaderPassthroughInboundMiddleware>(
            header => header.Key.StartsWith("X-Passthru-")
        );
    }
}
```
