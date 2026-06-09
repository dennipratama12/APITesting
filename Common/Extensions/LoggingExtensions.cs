using APITesting.Common.Configuration;
using Serilog;
using Serilog.Events;

namespace APITesting.Common.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        var opts = builder.Configuration
            .GetSection(AppLoggingOptions.SectionName)
            .Get<AppLoggingOptions>() ?? new AppLoggingOptions();

        var config = new LoggerConfiguration()
            .MinimumLevel.Is(ToLogEventLevel(opts.MinimumLevel));

        foreach (var (ns, level) in opts.LevelOverrides)
        {
            config.MinimumLevel.Override(ns, ToLogEventLevel(level));
        }

        config.Enrich.FromLogContext();

        if (opts.WriteToConsole)
        {
            config.WriteTo.Console();
        }

        if (opts.File.Enabled)
        {
            var logPath = Path.IsPathRooted(opts.File.Path)
                ? opts.File.Path
                : Path.Combine(builder.Environment.ContentRootPath, opts.File.Path);

            config.WriteTo.File(
                path: logPath,
                rollingInterval: ToRollingInterval(opts.File.RollingInterval),
                retainedFileCountLimit: opts.File.RetentionDays,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
        }

        Log.Logger = config.CreateLogger();
        builder.Host.UseSerilog();

        return builder;
    }

    private static LogEventLevel ToLogEventLevel(string level) =>
        Enum.TryParse<LogEventLevel>(level, ignoreCase: true, out var parsed)
            ? parsed
            : LogEventLevel.Information;

    private static RollingInterval ToRollingInterval(string interval) =>
        Enum.TryParse<RollingInterval>(interval, ignoreCase: true, out var parsed)
            ? parsed
            : RollingInterval.Day;
}
