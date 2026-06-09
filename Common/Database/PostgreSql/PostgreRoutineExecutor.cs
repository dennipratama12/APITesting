using System.Data;
using System.Text;
using APITesting.Common.Configuration;
using APITesting.Common.Constants;
using Microsoft.Extensions.Options;
using Npgsql;

namespace APITesting.Common.Database.PostgreSql;

public interface IPostgreRoutineExecutor
{
    Task<T?> ExecuteScalarAsync<T>(
        PostgreCallInfo callInfo,
        string? databaseKey = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ExecuteReaderAsync<T>(
        PostgreCallInfo callInfo,
        Func<IDataRecord, T> mapper,
        string? databaseKey = null,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteNonQueryAsync(
        PostgreCallInfo callInfo,
        string? databaseKey = null,
        CancellationToken cancellationToken = default);
}

public sealed class PostgreRoutineExecutor(
    IPostgreConnectionFactory connectionFactory,
    IOptions<PostgreSqlOptions> options) : IPostgreRoutineExecutor
{
    public async Task<T?> ExecuteScalarAsync<T>(
        PostgreCallInfo callInfo,
        string? databaseKey = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection(databaseKey);
        await connection.OpenAsync(cancellationToken);

        await using var command = BuildCommand(connection, callInfo);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result is null or DBNull)
        {
            return default;
        }

        if (result is T typedResult)
        {
            return typedResult;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<IReadOnlyList<T>> ExecuteReaderAsync<T>(
        PostgreCallInfo callInfo,
        Func<IDataRecord, T> mapper,
        string? databaseKey = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection(databaseKey);
        await connection.OpenAsync(cancellationToken);

        await using var command = BuildCommand(connection, callInfo);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<T>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(mapper(reader));
        }

        return items;
    }

    public async Task<int> ExecuteNonQueryAsync(
        PostgreCallInfo callInfo,
        string? databaseKey = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection(databaseKey);
        await connection.OpenAsync(cancellationToken);

        await using var command = BuildCommand(connection, callInfo);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private NpgsqlCommand BuildCommand(NpgsqlConnection connection, PostgreCallInfo callInfo)
    {
        if (string.IsNullOrWhiteSpace(callInfo.RoutineName))
        {
            throw new ArgumentException(GlobalParams.Errors.RoutineNameRequired, nameof(callInfo));
        }

        var command = connection.CreateCommand();
        command.CommandTimeout = options.Value.DefaultCommandTimeoutSeconds;
        command.CommandText = BuildCommandText(callInfo);
        command.CommandType = CommandType.Text;

        foreach (var parameter in callInfo.Parameters)
        {
            var dbParameter = command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);

            if (parameter.DbType.HasValue)
            {
                dbParameter.NpgsqlDbType = parameter.DbType.Value;
            }
        }

        return command;
    }

    private static string BuildCommandText(PostgreCallInfo callInfo)
    {
        var parameterNames = callInfo.Parameters.Select(p => $"@{p.Name.TrimStart('@')}");
        var arguments = string.Join(", ", parameterNames);

        return callInfo.CallType switch
        {
            PostgreCallType.Function => $"SELECT * FROM {QuoteRoutine(callInfo.RoutineName)}({arguments});",
            PostgreCallType.Procedure => $"CALL {QuoteRoutine(callInfo.RoutineName)}({arguments});",
            _ => throw new NotSupportedException(GlobalParams.Errors.UnsupportedCallType(callInfo.CallType))
        };
    }

    private static string QuoteRoutine(string routineName)
    {
        var parts = routineName
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => $"\"{part.Replace("\"", "\"\"")}\"");

        return string.Join('.', parts);
    }
}
