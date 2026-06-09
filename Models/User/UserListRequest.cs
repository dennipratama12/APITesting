using System.ComponentModel.DataAnnotations;

namespace APITesting.Models.User;

public sealed class UserListRequest
{
    [StringLength(100, ErrorMessage = "Keyword maksimal 100 karakter.")]
    public string? Keyword { get; init; }

    [StringLength(50, ErrorMessage = "Role maksimal 50 karakter.")]
    public string? Role { get; init; }

    public bool? IsActive { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page harus lebih besar dari 0.")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize harus di antara 1 sampai 100.")]
    public int PageSize { get; init; } = 20;

    public int Offset => (Page - 1) * PageSize;
}
