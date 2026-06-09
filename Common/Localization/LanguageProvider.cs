using System.Text.Json;
using APITesting.Common.Constants;

namespace APITesting.Common.Localization;

public sealed class LanguageProvider(
    IWebHostEnvironment environment,
    IHttpContextAccessor httpContextAccessor)
{
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> languages =
        LoadLanguages(environment);

    public string Get(string key)
    {
        return Get(
            GetCurrentLanguage(),
            key
        );
    }

    public string Get(string? language, string key)
    {
        var selectedLanguage = NormalizeLanguage(language);

        if (TryGetValue(selectedLanguage, key, out var value))
            return value;

        if (TryGetValue(GlobalParams.App.DefaultLanguage, key, out var defaultValue))
            return defaultValue;

        return key;
    }

    private string GetCurrentLanguage()
    {
        var language = httpContextAccessor.HttpContext?
            .Request.Headers[GlobalParams.Headers.Language]
            .FirstOrDefault();

        return NormalizeLanguage(language);
    }

    private static string NormalizeLanguage(string? language)
    {
        return string.IsNullOrWhiteSpace(language)
            ? GlobalParams.App.DefaultLanguage
            : language.Trim();
    }

    private bool TryGetValue(string language, string key, out string value)
    {
        value = string.Empty;

        if (!languages.TryGetValue(language, out var selectedLanguage))
            return false;

        return selectedLanguage.TryGetValue(key, out value!);
    }

    private static Dictionary<string, IReadOnlyDictionary<string, string>> LoadLanguages(
        IWebHostEnvironment environment)
    {
        var path = Path.Combine(
            environment.ContentRootPath,
            GlobalParams.App.LanguageSettingsFile);

        var result = new Dictionary<string, IReadOnlyDictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(path))
            return result;

        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);

        if (!document.RootElement.TryGetProperty(
                GlobalParams.App.LanguagesSection,
                out var languagesElement))
        {
            return result;
        }

        foreach (var language in languagesElement.EnumerateObject())
        {
            var values = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in language.Value.EnumerateObject())
            {
                values[item.Name] = item.Value.GetString() ?? item.Name;
            }

            result[language.Name] = values;
        }

        return result;
    }
}
