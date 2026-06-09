using APITesting.Common.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace APITesting.Common.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApplicationSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SwaggerOptions.SectionName).Get<SwaggerOptions>() ?? new SwaggerOptions();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc(options.Version, new OpenApiInfo
            {
                Title = options.Title,
                Version = options.Version,
                Description = options.Description
            });
        });

        return services;
    }

    public static IApplicationBuilder UseApplicationSwagger(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        if (!options.Enabled && !app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint($"/swagger/{options.Version}/swagger.json", $"{options.Title} {options.Version}");
            setup.RoutePrefix = "docs";

            setup.InjectStylesheet("/swagger-ui/custom-swagger.css");
            setup.InjectJavascript("/swagger-ui/custom-swagger.js");

            setup.EnableFilter();
            setup.DisplayRequestDuration();
            setup.EnableDeepLinking();
        });

        return app;
    }
}
