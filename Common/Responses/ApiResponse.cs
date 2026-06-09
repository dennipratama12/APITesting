using System.Text.Json;
using System.Text.Json.Serialization;
using APITesting.Common.Constants;
using APITesting.Common.Localization;

namespace APITesting.Common.Responses;

public sealed class ApiResponse
{
    [JsonPropertyName(GlobalParams.Json.Status)]
    public string Status { get; private set; }

    [JsonPropertyName(GlobalParams.Json.Message)]
    public string Message { get; private set; }

    [JsonPropertyName(GlobalParams.Json.Data)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; private set; }

    [JsonPropertyName(GlobalParams.Json.Errors)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Errors { get; private set; }

    // Field dinamis — key apapun akan muncul sebagai top-level JSON property
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; private set; }

    // Untuk controller — language di-resolve langsung di konstruktor
    public ApiResponse(LanguageProvider language, string statusKey, string messageKey)
    {
        Status = language.Get(statusKey);
        Message = language.Get(messageKey);
    }

    // Untuk middleware — raw string langsung
    public ApiResponse(string status, string message, object? data = null, object? errors = null)
    {
        Status = status;
        Message = message;
        Data = data;
        Errors = errors;
    }

    // Factory:
    // - T di-infer dari valueFactoryAsync
    // - dataSelector opsional: kalau perlu extract sebagian dari T sebagai data
    // - extraSelector opsional: tambah field dinamis sesuai kebutuhan
    public static async Task<ApiResponse> AddAsync<T>(
        LanguageProvider language,
        string status,
        string message,
        Func<CancellationToken, Task<T>> valueFactoryAsync,
        Func<T, object?>? dataSelector = null,
        Func<T, Dictionary<string, object?>>? extraSelector = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ApiResponse(language, status, message);
        var value = await valueFactoryAsync(cancellationToken);

        response.Data = dataSelector is not null ? dataSelector(value) : value;

        if (extraSelector is not null)
        {
            var extra = extraSelector(value);
            response.Extra = extra.ToDictionary(
                kvp => kvp.Key,
                kvp => JsonSerializer.SerializeToElement(kvp.Value));
        }

        return response;
    }
}
