using NpgsqlTypes;

namespace APITesting.Common.Database.PostgreSql;

public enum PostgreCallType
{
    Function = 1,
    Procedure = 2
}

public sealed class PostgreCallInfo
{
    public required string RoutineName { get; init; }

    public PostgreCallType CallType { get; init; } = PostgreCallType.Function;

    public IReadOnlyCollection<PostgreParameter> Parameters { get; init; } = [];
}

public sealed class PostgreParameter
{
    public required string Name { get; init; }

    public object? Value { get; init; }

    public NpgsqlDbType? DbType { get; init; }
}
