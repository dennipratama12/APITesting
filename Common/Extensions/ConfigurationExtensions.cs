using APITesting.Common.Configuration;
using APITesting.Common.Constants;

namespace APITesting.Common.Extensions;

public static class ConfigurationExtensions
{
    public static WebApplicationBuilder AddApplicationConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile(GlobalParams.App.LanguageSettingsFile, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder;
    }

    public static void ValidateApplicationConfiguration(this IConfiguration configuration)
    {
        var mainConnectionString = configuration.GetConnectionString(GlobalParams.Database.Main);

        if (string.IsNullOrWhiteSpace(mainConnectionString))
        {
            throw new InvalidOperationException(
                $"{GlobalParams.Config.ConnectionStringPath(GlobalParams.Database.Main)} is required.");
        }

        var postgreSqlOptions = configuration
            .GetSection(PostgreSqlOptions.SectionName)
            .Get<PostgreSqlOptions>() ?? new PostgreSqlOptions();

        if (postgreSqlOptions.DefaultCommandTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"{PostgreSqlOptions.SectionName}:DefaultCommandTimeoutSeconds must be greater than 0.");
        }

        var loggingOptions = configuration
            .GetSection(AppLoggingOptions.SectionName)
            .Get<AppLoggingOptions>() ?? new AppLoggingOptions();

        if (loggingOptions.File.Enabled && string.IsNullOrWhiteSpace(loggingOptions.File.Path))
        {
            throw new InvalidOperationException(
                $"{AppLoggingOptions.SectionName}:File:Path is required when file logging is enabled.");
        }

        if (loggingOptions.File.RetentionDays <= 0)
        {
            throw new InvalidOperationException(
                $"{AppLoggingOptions.SectionName}:File:RetentionDays must be greater than 0.");
        }
    }
}
