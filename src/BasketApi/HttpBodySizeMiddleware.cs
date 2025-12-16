using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

namespace BasketApi;

public class HttpBodySizeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpBodySizeMiddleware> _logger;

    public HttpBodySizeMiddleware(RequestDelegate next, ILogger<HttpBodySizeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var activityFeature = context.Features.Get<IHttpActivityFeature>();
        var activity = activityFeature?.Activity;

        // Capture request body size
        long requestBodySize = 0;
        if (context.Request.ContentLength.HasValue)
        {
            requestBodySize = context.Request.ContentLength.Value;
        }
        else if (context.Request.Body.CanSeek)
        {
            requestBodySize = context.Request.Body.Length;
        }

        // Add request body size to activity
        if (requestBodySize > 0)
        {
            activity?.SetTag("http.request.body.size", requestBodySize);
        }

        // To capture response body size, we need to replace the response stream
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            // Call the next middleware in the pipeline
            await _next(context);

            // Capture response body size
            var responseBodySize = responseBodyStream.Length;

            // Add response body size to activity
            if (responseBodySize > 0)
            {
                activity?.SetTag("http.response.body.size", responseBodySize);
            }

            // Copy the response back to the original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);
        }
        finally
        {
            // Restore the original response body stream
            context.Response.Body = originalResponseBody;
        }
    }
}

// Extension method to make it easy to add the middleware
public static class HttpBodySizeMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpBodySizeTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpBodySizeMiddleware>();
    }
}
