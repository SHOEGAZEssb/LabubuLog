using System.Text.Json;
using System.Text.RegularExpressions;

namespace LabubuLog.Services.GameMetadata;

public partial class SteamGameMetadataProvider(HttpClient httpClient) : IGameMetadataProvider
{
    private const string Language = "english";
    private const string CountryCode = "US";

    public string Name => "Steam";

    public async Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var url = $"https://store.steampowered.com/api/storesearch/?term={Uri.EscapeDataString(searchTerm.Trim())}&l={Language}&cc={CountryCode}";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return items.EnumerateArray()
            .Where(item => TryGetString(item, "type") == "app")
            .Select(item =>
            {
                var appId = TryGetInt(item, "id")?.ToString();
                var name = TryGetString(item, "name");

                if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }

                return new GameMetadataSearchResult(
                    Name,
                    appId,
                    name,
                    TryGetString(item, "tiny_image"),
                    FormatPlatforms(item));
            })
            .Where(result => result is not null)
            .Cast<GameMetadataSearchResult>()
            .Take(8)
            .ToList();
    }

    public async Task<GameMetadataDetails?> GetDetailsAsync(string providerId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(providerId, out var appId))
        {
            return null;
        }

        var url = $"https://store.steampowered.com/api/appdetails?appids={appId}&l={Language}&cc={CountryCode}";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty(providerId, out var appNode) ||
            !TryGetBool(appNode, "success") ||
            !appNode.TryGetProperty("data", out var data))
        {
            return null;
        }

        if (TryGetString(data, "type") != "game")
        {
            return null;
        }

        var title = TryGetString(data, "name");

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        return new GameMetadataDetails(
            Name,
            providerId,
            title,
            FormatPlatforms(data) ?? "Steam",
            FormatGenres(data),
            ParseReleaseYear(data),
            BuildSteamLibraryCoverUrl(appId),
            TryGetString(data, "header_image") ?? TryGetString(data, "capsule_imagev5") ?? TryGetString(data, "capsule_image"),
            "Steam");
    }

    private static string? FormatPlatforms(JsonElement item)
    {
        if (!item.TryGetProperty("platforms", out var platforms))
        {
            return null;
        }

        var names = new List<string>();

        if (TryGetBool(platforms, "windows"))
        {
            names.Add("Windows");
        }

        if (TryGetBool(platforms, "mac"))
        {
            names.Add("Mac");
        }

        if (TryGetBool(platforms, "linux"))
        {
            names.Add("Linux");
        }

        return names.Count == 0 ? "Steam" : $"{string.Join(", ", names)} (Steam)";
    }

    private static string? FormatGenres(JsonElement data)
    {
        if (!data.TryGetProperty("genres", out var genres) || genres.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var names = genres.EnumerateArray()
            .Select(genre => TryGetString(genre, "description"))
            .Where(description => !string.IsNullOrWhiteSpace(description))
            .Take(3)
            .ToList();

        return names.Count == 0 ? null : string.Join(", ", names);
    }

    private static int? ParseReleaseYear(JsonElement data)
    {
        if (!data.TryGetProperty("release_date", out var releaseDate))
        {
            return null;
        }

        var date = TryGetString(releaseDate, "date");

        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        var match = YearRegex().Match(date);
        return match.Success && int.TryParse(match.Value, out var year) ? year : null;
    }

    private static string BuildSteamLibraryCoverUrl(int appId)
    {
        return $"https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{appId}/library_600x900.jpg";
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

    private static bool TryGetBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind is JsonValueKind.True or JsonValueKind.False &&
            property.GetBoolean();
    }

    [GeneratedRegex(@"\b(19|20)\d{2}\b")]
    private static partial Regex YearRegex();
}
