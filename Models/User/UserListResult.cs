namespace APITesting.Models.User;

public sealed class UserListResult
{
    public IReadOnlyList<UserRowResponse> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalData { get; init; }

    public object ToMeta() => new
    {
        page = Page,
        pageSize = PageSize,
        totalData = TotalData,
        totalPage = PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalData / (double)PageSize)
    };
}
