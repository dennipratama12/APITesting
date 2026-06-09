namespace APITesting.Common.Responses;

public sealed class ValidationErrorItem
{
    public required string Field { get; init; }

    public required string Message { get; init; }
}
