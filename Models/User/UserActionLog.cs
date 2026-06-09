namespace APITesting.Models.User;

public sealed class UserActionLog
{
    public Guid? UserId { get; init; }
    public required string Action { get; init; }
    public string? RequestData { get; init; }
    public string? IpAddress { get; init; }
    public string? TraceId { get; init; }
}
