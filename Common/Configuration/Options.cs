using APITesting.Common.Constants;

namespace APITesting.Common.Configuration;

public sealed class PostgreSqlOptions
{
    public const string SectionName = GlobalParams.Config.PostgreSql;

    public int DefaultCommandTimeoutSeconds { get; init; } = 30;
}

public sealed class SwaggerOptions
{
    public const string SectionName = GlobalParams.Config.Swagger;

    public bool Enabled { get; init; } = true;
    public string Title { get; init; } = GlobalParams.App.Name;
    public string Version { get; init; } = GlobalParams.App.DefaultApiVersion;
    public string Description { get; init; } = GlobalParams.App.Description;

}

public sealed class SecurityHeadersOptions
{
    public const string SectionName = GlobalParams.Config.SecurityHeaders;

    public bool Enabled { get; init; } = true;
    public string XContentTypeOptions { get; init; } = "nosniff";
    public string XFrameOptions { get; init; } = "DENY";
    public string XXssProtection { get; init; } = "1; mode=block";
    public string ReferrerPolicy { get; init; } = "no-referrer";
    public string ContentSecurityPolicy { get; init; } = "default-src 'self'";
    public string PermissionsPolicy { get; init; } = "camera=(), microphone=(), geolocation=()";
}

public sealed class AppLoggingOptions
{
    public const string SectionName = GlobalParams.Config.AppLogging;

    public string MinimumLevel { get; init; } = "Information";
    public bool WriteToConsole { get; init; } = true;
    public FileLogOptions File { get; init; } = new();
    public RequestLogOptions Request { get; init; } = new();
    public Dictionary<string, string> LevelOverrides { get; init; } = new();
}

public sealed class FileLogOptions
{
    public bool Enabled { get; init; } = true;
    public string Path { get; init; } = "wwwroot/logs/APITesting-.log";
    public string RollingInterval { get; init; } = "Day";
    public int RetentionDays { get; init; } = 14;
}

public sealed class RequestLogOptions
{
    public bool LogRequestBody { get; init; }
    public bool LogResponseBody { get; init; }
    public int MaxBodyLength { get; init; } = 4000;
    public string[] SensitiveFields { get; init; } = [];
}
