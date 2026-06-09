namespace APITesting.Common.Constants;

public static class GlobalParams
{
    public static class App
    {
        public const string Name = "APITesting";
        public const string Description = "Dokumentasi API Testing. Built with ASP.NET Core.";
        public const string DefaultApiVersion = "v1";
        public const string DefaultLanguage = "id";
        public const string LanguageSettingsFile = "appsettings.languages.json";
        public const string LanguagesSection = "Languages";
        public const string TruncatedSuffix = "... [truncated]";
    }

    public static class Config
    {
        public const string ConnectionStrings = "ConnectionStrings";
        public const string PostgreSql = "PostgreSql";
        public const string AppLogging = "AppLogging";
        public const string Swagger = "Swagger";
        public const string SecurityHeaders = "SecurityHeaders";

        public static string ConnectionStringPath(string name) => $"{ConnectionStrings}:{name}";
    }

    public static class Database
    {
        public const string Main = "Main";
        public const string Log = "Log";
    }

    public static class Headers
    {
        public const string Language = "language";
    }

    public static class ContentType
    {
        public const string ApplicationJson = "application/json";
        public const string Text = "text/";
        public const string ApplicationXml = "application/xml";
    }

    public static class Json
    {
        public const string Status = "status";
        public const string Message = "message";
        public const string Data = "data";
        public const string Meta = "meta";
        public const string Errors = "errors";
    }

    public static class Security
    {
        public const string SensitiveValueMask = "***";
    }

    public static class Logs
    {
        public const string AppStarting = "Starting APITesting application.";
        public const string AppStartFailed = "APITesting application failed to start.";
        public const string RequestCancelled = "Request cancelled by client. TraceId: {TraceId}";
        public const string UnhandledException = "Unhandled exception. TraceId: {TraceId}";
        public const string HttpRequest = "HTTP {Method} {Url} responded {StatusCode} in {ElapsedMs}ms | Req: {RequestBody} | Res: {ResponseBody}";
    }

    public static class Errors
    {
        public const string RoutineNameRequired = "Routine name is required.";

        public static string ConnectionNotConfigured(string key) =>
            $"PostgreSQL connection string for '{Config.ConnectionStringPath(key)}' is not configured.";

        public static string UnsupportedCallType(object callType) =>
            $"PostgreSQL call type '{callType}' is not supported.";
    }

    public static class Lang
    {
        public static class Status
        {
            public const string Success = "api.status.success";
            public const string Failed = "api.status.failed";
        }

        public static class Message
        {
            public const string Success = "api.message.success";
            public const string Created = "api.message.created";
            public const string Updated = "api.message.updated";
            public const string Deleted = "api.message.deleted";
            public const string BadRequest = "api.message.bad_request";
            public const string ValidationFailed = "api.message.validation_failed";
            public const string NotFound = "api.message.not_found";
            public const string Unauthorized = "api.message.unauthorized";
            public const string Conflict = "api.message.conflict";
            public const string UnprocessableEntity = "api.message.unprocessable_entity";
            public const string Timeout = "api.message.timeout";
            public const string NotImplemented = "api.message.not_implemented";
            public const string InternalServerError = "api.message.internal_server_error";
        }
    }
}
