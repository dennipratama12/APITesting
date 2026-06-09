using APITesting.Common.Constants;
using APITesting.Common.Database.PostgreSql;
using APITesting.Models.User;
using NpgsqlTypes;

namespace APITesting.Repositories.User;

public interface IUserLogRepository
{
    Task InsertAsync(UserActionLog log, CancellationToken cancellationToken = default);
}

public sealed class UserLogRepository(IPostgreRoutineExecutor executor) : IUserLogRepository
{
    private const string DatabaseKey = GlobalParams.Database.Log;

    public Task InsertAsync(UserActionLog log, CancellationToken cancellationToken = default)
    {
        var callInfo = new PostgreCallInfo
        {
            RoutineName = "log.sp_user_action_log_insert",
            CallType = PostgreCallType.Procedure,
            Parameters =
            [
                new PostgreParameter { Name = "p_user_id", Value = log.UserId, DbType = NpgsqlDbType.Uuid },
                new PostgreParameter { Name = "p_action", Value = log.Action, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_request_data", Value = log.RequestData, DbType = NpgsqlDbType.Jsonb },
                new PostgreParameter { Name = "p_ip_address", Value = log.IpAddress, DbType = NpgsqlDbType.Varchar },
                new PostgreParameter { Name = "p_trace_id", Value = log.TraceId, DbType = NpgsqlDbType.Varchar }
            ]
        };

        return executor.ExecuteNonQueryAsync(callInfo, DatabaseKey, cancellationToken);
    }
}
