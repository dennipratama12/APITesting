using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APITesting.Models.User;

public sealed class UpdateUserRequest
{
    [Required(ErrorMessage = "Email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [StringLength(255, ErrorMessage = "Email maksimal 255 karakter.")]
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(200, ErrorMessage = "Nama lengkap maksimal 200 karakter.")]
    [JsonPropertyName("full_name")]
    public string FullName { get; init; } = string.Empty;

    [Range(62000000000, 62999999999, ErrorMessage = "Nomor telepon harus diawali 62 dan total 12 digit.")]
    [JsonPropertyName("phone_number")]
    public long? PhoneNumber { get; init; }

    [Required(ErrorMessage = "Role wajib diisi.")]
    [StringLength(50, ErrorMessage = "Role maksimal 50 karakter.")]
    [JsonPropertyName("role")]
    public string Role { get; init; } = "user";

    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; } = true;
}
