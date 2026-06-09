using APITesting.Common.Logging;
using APITesting.Common.Middlewares;
using Microsoft.Extensions.FileProviders;

namespace APITesting.Common.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.WebRootPath, "swagger-ui")),
            RequestPath = "/swagger-ui"
        });

        app.UseHttpsRedirection();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.MapControllers();

        return app;
    }
}
