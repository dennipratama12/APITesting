using System.Text.Json;
using APITesting.Models.User;
using APITesting.Repositories.User;

namespace APITesting.Services.User;

public interface IUserService
{
    Task<UserRowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserListResult> GetListAsync(UserListRequest request, CancellationToken cancellationToken = default);
    Task<UserRowResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserRowResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class UserService(
    IUserRepository repository,
    IUserLogRepository logRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserService> logger) : IUserService
{
    private static class Action
    {
        public const string GetDetail = "GET_DETAIL";
        public const string GetList = "GET_LIST";
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
    }

    public async Task<UserRowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User dengan id '{id}' tidak ditemukan.");

        await TryLogAsync(BuildLog(Action.GetDetail, userId: result.Id), cancellationToken);
        return result;
    }

    public async Task<UserListResult> GetListAsync(UserListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetListAsync(request, cancellationToken);
        await TryLogAsync(BuildLog(Action.GetList, requestData: request), cancellationToken);
        return result;
    }

    public async Task<UserRowResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await repository.CreateAsync(request, cancellationToken)
            ?? throw new InvalidOperationException("Gagal membuat user baru.");

        await TryLogAsync(BuildLog(Action.Create, userId: result.Id, requestData: request), cancellationToken);
        return result;
    }

    public async Task<UserRowResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await repository.UpdateAsync(id, request, cancellationToken)
            ?? throw new KeyNotFoundException($"User dengan id '{id}' tidak ditemukan.");

        await TryLogAsync(BuildLog(Action.Update, userId: id, requestData: request), cancellationToken);
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            throw new KeyNotFoundException($"User dengan id '{id}' tidak ditemukan atau sudah tidak aktif.");
        }

        await TryLogAsync(BuildLog(Action.Delete, userId: id), cancellationToken);
    }

    private UserActionLog BuildLog(string action, Guid? userId = null, object? requestData = null) => new()
    {
        Action = action,
        UserId = userId,
        RequestData = requestData is not null ? JsonSerializer.Serialize(requestData) : null,
        IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
        TraceId = httpContextAccessor.HttpContext?.TraceIdentifier
    };

    private async Task TryLogAsync(UserActionLog log, CancellationToken cancellationToken)
    {
        try
        {
            await logRepository.InsertAsync(log, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gagal insert user action log. Action: {Action}, UserId: {UserId}", log.Action, log.UserId);
        }
    }
}
