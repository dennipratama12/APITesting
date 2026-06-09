using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APITesting.Models.User;

public sealed class CreateUserRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(100, ErrorMessage = "Username maksimal 100 karakter.")]
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

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
}
