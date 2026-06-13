using System.Text.Json;

namespace LabubuLog.Services.GameMetadata;

public class RawgGameMetadataProvider(HttpClient httpClient, IConfiguration configuration) : IGameMetadataProvider
{
    private const string BaseUrl = "https://api.rawg.io/api";
    private const int SearchPageSize = 8;

    public string Name => "RAWG";

    public async Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        var apiKey = configuration["GameMetadata:RawgApiKey"];

        if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(apiKey))
        {
            return [];
        }

        var url = $"{BaseUrl}/games?key={Uri.EscapeDataString(apiKey)}&search={Uri.EscapeDataString(searchTerm.Trim())}&page_size={SearchPageSize}&search_precise=true";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return results.EnumerateArray()
            .Select(result =>
            {
                var id = TryGetInt(result, "id")?.ToString();
                var title = TryGetString(result, "name");

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
                {
                    return null;
                }

                return new GameMetadataSearchResult(
                    Name,
                    id,
                    title,
                    TryGetString(result, "background_image"),
                    FormatPlatforms(result));
            })
            .Where(result => result is not null)
            .Cast<GameMetadataSearchResult>()
            .ToList();
    }

    public async Task<GameMetadataDetails?> GetDetailsAsync(string providerId, CancellationToken cancellationToken)
    {
        var apiKey = configuration["GameMetadata:RawgApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(providerId))
        {
            return null;
        }

        var url = $"{BaseUrl}/games/{Uri.EscapeDataString(providerId)}?key={Uri.EscapeDataString(apiKey)}";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var data = document.RootElement;
        var title = TryGetString(data, "name");

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var backgroundImage = TryGetString(data, "background_image");
        var additionalImage = TryGetString(data, "background_image_additional");

        return new GameMetadataDetails(
            Name,
            providerId,
            title,
            FormatPlatforms(data),
            FormatGenres(data),
            ParseReleaseYear(data),
            backgroundImage ?? additionalImage,
            additionalImage ?? backgroundImage,
            "RAWG");
    }

    private static string? FormatPlatforms(JsonElement data)
    {
        if (!data.TryGetProperty("platforms", out var platforms) || platforms.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var names = platforms.EnumerateArray()
            .Select(platform => platform.TryGetProperty("platform", out var platformData)
                ? TryGetString(platformData, "name")
                : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        return names.Count == 0 ? null : string.Join(", ", names);
    }

    private static string? FormatGenres(JsonElement data)
    {
        if (!data.TryGetProperty("genres", out var genres) || genres.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var names = genres.EnumerateArray()
            .Select(genre => TryGetString(genre, "name"))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        return names.Count == 0 ? null : string.Join(", ", names);
    }

    private static int? ParseReleaseYear(JsonElement data)
    {
        var released = TryGetString(data, "released");

        if (string.IsNullOrWhiteSpace(released) || released.Length < 4)
        {
            return null;
        }

        return int.TryParse(released[..4], out var year) ? year : null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? TryGetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : null;
    }
}
