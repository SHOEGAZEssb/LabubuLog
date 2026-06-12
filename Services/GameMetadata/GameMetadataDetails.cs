namespace LabubuLog.Services.GameMetadata;

public record GameMetadataDetails(
    string Provider,
    string ProviderId,
    string Title,
    string? Platform,
    string? Genre,
    int? ReleaseYear,
    string? CoverImageUrl,
    string? TitleImageUrl,
    string? Tags);
