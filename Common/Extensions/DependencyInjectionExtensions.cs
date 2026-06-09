using APITesting.Common.Configuration;
using APITesting.Common.Database.PostgreSql;
using APITesting.Common.Localization;
using APITesting.Common.Logging;
using APITesting.Repositories.User;
using APITesting.Services.User;

namespace APITesting.Common.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SwaggerOptions>(configuration.GetSection(SwaggerOptions.SectionName));
        services.Configure<SecurityHeadersOptions>(configuration.GetSection(SecurityHeadersOptions.SectionName));
        services.Configure<AppLoggingOptions>(configuration.GetSection(AppLoggingOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddSingleton<LanguageProvider>();
        services.AddSingleton<RequestBodyReader>();

        services.AddScoped<IPostgreConnectionFactory, PostgreConnectionFactory>();
        services.AddScoped<IPostgreRoutineExecutor, PostgreRoutineExecutor>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserLogRepository, UserLogRepository>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
