using System.Data;
using APITesting.Common.Constants;
using APITesting.Common.Database.PostgreSql;
using APITesting.Models.User;
using NpgsqlTypes;

namespace APITesting.Repositories.User;

public interface IUserRepository
{
    Task<UserRowResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserListResult> GetListAsync(UserListRequest request, CancellationToken cancellationToken = default);
    Task<UserRowResponse?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserRowResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class UserRepository(IPostgreRoutineExecutor executor) : IUserRepository
{
    private const string DatabaseKey = GlobalParams.Database.Main;

    public async Task<UserRowResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var callInfo = new PostgreCallInfo
        {
            RoutineName = "public.fn_user_get_by_id",
            CallType = PostgreCallType.Function,
            Parameters =
            [
                new PostgreParameter { Name = "p_id", Value = id, DbType = NpgsqlDbType.Uuid }
            ]
        };

        var rows = await executor.ExecuteReaderAsync(callInfo, MapUser, DatabaseKey, cancellationToken);
        return rows.Count > 0 ? rows[0] : null;
    }

    public async Task<UserListResult> GetListAsync(UserListRequest request, CancellationToken cancellationToken = default)
    {
        var listCallInfo = new PostgreCallInfo
        {
            RoutineName = "public.fn_user_get_list",
            CallType = PostgreCallType.Function,
            Parameters =
            [
                new PostgreParameter { Name = "p_keyword", Value = request.Keyword, DbType = NpgsqlDbType.Text },
                new PostgreParameter { Name = "p_role", Value = request.Role, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_is_active", Value = request.IsActive, DbType = NpgsqlDbType.Boolean },
                new PostgreParameter { Name = "p_limit", Value = request.PageSize, DbType = NpgsqlDbType.Integer },
                new PostgreParameter { Name = "p_offset", Value = request.Offset, DbType = NpgsqlDbType.Integer }
            ]
        };

        var countCallInfo = new PostgreCallInfo
        {
            RoutineName = "public.fn_user_count",
            CallType = PostgreCallType.Function,
            Parameters =
            [
                new PostgreParameter { Name = "p_keyword", Value = request.Keyword, DbType = NpgsqlDbType.Text },
                new PostgreParameter { Name = "p_role", Value = request.Role, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_is_active", Value = request.IsActive, DbType = NpgsqlDbType.Boolean }
            ]
        };

        var items = await executor.ExecuteReaderAsync(listCallInfo, MapUser, DatabaseKey, cancellationToken);
        var totalData = await executor.ExecuteScalarAsync<int>(countCallInfo, DatabaseKey, cancellationToken);

        return new UserListResult
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalData = totalData
        };
    }

    public async Task<UserRowResponse?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var callInfo = new PostgreCallInfo
        {
            RoutineName = "public.fn_user_create",
            CallType = PostgreCallType.Function,
            Parameters =
            [
                new PostgreParameter { Name = "p_username", Value = request.Username, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_email", Value = request.Email, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_full_name", Value = request.FullName, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_phone_number", Value = request.PhoneNumber, DbType = NpgsqlDbType.Bigint },
                new PostgreParameter { Name = "p_role", Value = request.Role, DbType = NpgsqlDbType.Varchar }
            ]
        };

        var rows = await executor.ExecuteReaderAsync(callInfo, MapUser, DatabaseKey, cancellationToken);
        return rows.Count > 0 ? rows[0] : null;
    }

    public async Task<UserRowResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var callInfo = new PostgreCallInfo
        {
            RoutineName = "public.fn_user_update",
            CallType = PostgreCallType.Function,
            Parameters =
            [
                new PostgreParameter { Name = "p_id", Value = id, DbType = NpgsqlDbType.Uuid },
                new PostgreParameter { Name = "p_email", Value = request.Email, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_full_name", Value = request.FullName, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_phone_number", Value = request.PhoneNumber, DbType = NpgsqlDbType.Bigint },
                new PostgreParameter { Name = "p_role", Value = request.Role, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_is_active", Value = request.IsActive, DbType = NpgsqlDbType.Boolean }
            ]
        };

        var rows = await executor.ExecuteReaderAsync(callInfo, MapUser, DatabaseKey, cancellationToken);
        return rows.Count > 0 ? rows[0] : null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var callInfo = new PostgreCallInfo
        {
            RoutineName = "public.fn_user_delete",
            CallType = PostgreCallType.Function,
            Parameters =
            [
                new PostgreParameter { Name = "p_id", Value = id, DbType = NpgsqlDbType.Uuid }
            ]
        };

        return await executor.ExecuteScalarAsync<bool>(callInfo, DatabaseKey, cancellationToken);
    }

    private static UserRowResponse MapUser(IDataRecord record) => new()
    {
        Id = GetValue<Guid>(record, "id"),
        Username = GetValue<string>(record, "username") ?? string.Empty,
        Email = GetValue<string>(record, "email") ?? string.Empty,
        FullName = GetValue<string>(record, "full_name") ?? string.Empty,
        PhoneNumber = GetValue<long?>(record, "phone_number"),
        Role = GetValue<string>(record, "role") ?? string.Empty,
        IsActive = GetValue<bool>(record, "is_active"),
        CreatedAt = GetValue<DateTimeOffset>(record, "created_at"),
        UpdatedAt = GetValue<DateTimeOffset?>(record, "updated_at")
    };

    private static T? GetValue<T>(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);

        if (record.IsDBNull(ordinal))
        {
            return default;
        }

        var value = record.GetValue(ordinal);

        if (value is T typedValue)
        {
            return typedValue;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType == typeof(DateTimeOffset) && value is DateTime dateTime)
        {
            return (T)(object)new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        return (T)Convert.ChangeType(value, targetType);
    }
}
