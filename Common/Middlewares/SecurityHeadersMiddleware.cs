using APITesting.Common.Configuration;
using Microsoft.Extensions.Options;

namespace APITesting.Common.Middlewares;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeadersOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (options.Value.Enabled)
        {
            var headers = context.Response.Headers;
            var o = options.Value;

            headers.XContentTypeOptions = o.XContentTypeOptions;
            headers.XFrameOptions = o.XFrameOptions;
            headers.XXSSProtection = o.XXssProtection;
            headers["Referrer-Policy"] = o.ReferrerPolicy;
            headers.ContentSecurityPolicy = o.ContentSecurityPolicy;
            headers["Permissions-Policy"] = o.PermissionsPolicy;
        }

        await next(context);
    }
}
