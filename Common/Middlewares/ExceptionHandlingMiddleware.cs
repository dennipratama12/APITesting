using System.Net;
using System.Runtime.ExceptionServices;
using APITesting.Common.Constants;
using APITesting.Common.Localization;
using APITesting.Common.Responses;

namespace APITesting.Common.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    LanguageProvider languageProvider,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogWarning(GlobalParams.Logs.RequestCancelled, context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, GlobalParams.Logs.UnhandledException, context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        var language = context.Request.Headers[GlobalParams.Headers.Language].FirstOrDefault()
            ?? GlobalParams.App.DefaultLanguage;

        var statusCode = GetStatusCode(exception);

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = GlobalParams.ContentType.ApplicationJson;

        var response = new ApiResponse(
            status: languageProvider.Get(language, GlobalParams.Lang.Status.Failed),
            message: GetMessage(language, exception, statusCode),
            errors: BuildError(context, exception));

        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }

    private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
    {
        ArgumentNullException => HttpStatusCode.BadRequest,
        ArgumentException => HttpStatusCode.BadRequest,
        FormatException => HttpStatusCode.BadRequest,
        KeyNotFoundException => HttpStatusCode.NotFound,
        FileNotFoundException => HttpStatusCode.NotFound,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        InvalidOperationException => HttpStatusCode.Conflict,
        NotSupportedException => HttpStatusCode.UnprocessableEntity,
        TimeoutException => HttpStatusCode.RequestTimeout,
        NotImplementedException => HttpStatusCode.NotImplemented,
        _ => HttpStatusCode.InternalServerError
    };

    private string GetMessage(string language, Exception exception, HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => exception.Message,
        HttpStatusCode.NotFound => languageProvider.Get(language, GlobalParams.Lang.Message.NotFound),
        HttpStatusCode.Unauthorized => languageProvider.Get(language, GlobalParams.Lang.Message.Unauthorized),
        HttpStatusCode.Conflict => languageProvider.Get(language, GlobalParams.Lang.Message.Conflict),
        HttpStatusCode.UnprocessableEntity => languageProvider.Get(language, GlobalParams.Lang.Message.UnprocessableEntity),
        HttpStatusCode.RequestTimeout => languageProvider.Get(language, GlobalParams.Lang.Message.Timeout),
        HttpStatusCode.NotImplemented => languageProvider.Get(language, GlobalParams.Lang.Message.NotImplemented),
        _ => languageProvider.Get(language, GlobalParams.Lang.Message.InternalServerError)
    };

    private object BuildError(HttpContext context, Exception exception)
    {
        if (!environment.IsDevelopment())
        {
            return new { traceId = context.TraceIdentifier };
        }

        return new
        {
            traceId = context.TraceIdentifier,
            exception = exception.GetType().Name,
            detail = exception.Message,
            stackTrace = exception.StackTrace
        };
    }
}
