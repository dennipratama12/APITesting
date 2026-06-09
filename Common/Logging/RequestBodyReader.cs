using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using APITesting.Common.Configuration;
using APITesting.Common.Constants;
using Microsoft.Extensions.Options;

namespace APITesting.Common.Logging;

public sealed class RequestBodyReader(IOptions<AppLoggingOptions> options)
{
    public async Task<string?> ReadRequestBodyAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Request.LogRequestBody || !CanReadBody(context.Request.ContentType))
        {
            return null;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(cancellationToken);
        context.Request.Body.Position = 0;

        return Normalize(body);
    }

    public async Task<string?> ReadResponseBodyAsync(Stream responseBody, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Request.LogResponseBody)
        {
            return null;
        }

        responseBody.Position = 0;

        using var reader = new StreamReader(
            responseBody,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(cancellationToken);
        responseBody.Position = 0;

        return Normalize(body);
    }

    private string Normalize(string body)
    {
        var maxLength = options.Value.Request.MaxBodyLength;
        var masked = MaskSensitiveData(body);

        if (maxLength <= 0 || masked.Length <= maxLength)
        {
            return masked;
        }

        return string.Concat(masked.AsSpan(0, maxLength), GlobalParams.App.TruncatedSuffix);
    }

    private string MaskSensitiveData(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        try
        {
            var jsonNode = JsonNode.Parse(content);

            if (jsonNode is null)
            {
                return content;
            }

            MaskNode(jsonNode);
            return jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            return content;
        }
    }

    private void MaskNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (IsSensitiveField(property.Key))
                {
                    jsonObject[property.Key] = GlobalParams.Security.SensitiveValueMask;
                    continue;
                }

                if (property.Value is not null)
                {
                    MaskNode(property.Value);
                }
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    MaskNode(item);
                }
            }
        }
    }

    private bool IsSensitiveField(string field)
    {
        return options.Value.Request.SensitiveFields.Any(sensitive =>
            string.Equals(sensitive, field, StringComparison.OrdinalIgnoreCase));
    }

    private static bool CanReadBody(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Contains(GlobalParams.ContentType.ApplicationJson, StringComparison.OrdinalIgnoreCase)
            || contentType.Contains(GlobalParams.ContentType.Text, StringComparison.OrdinalIgnoreCase)
            || contentType.Contains(GlobalParams.ContentType.ApplicationXml, StringComparison.OrdinalIgnoreCase);
    }
}
