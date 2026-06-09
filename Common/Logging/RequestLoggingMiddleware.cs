using System.Diagnostics;
using APITesting.Common.Constants;

namespace APITesting.Common.Logging;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger,
    RequestBodyReader bodyReader)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = await bodyReader.ReadRequestBodyAsync(context, context.RequestAborted);

        var originalResponseBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var responseBodyText = await bodyReader.ReadResponseBodyAsync(responseBody, context.RequestAborted);
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalResponseBody, context.RequestAborted);
            context.Response.Body = originalResponseBody;

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

            logger.LogInformation(
                GlobalParams.Logs.HttpRequest,
                context.Request.Method,
                url,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestBody,
                responseBodyText);
        }
    }
}
