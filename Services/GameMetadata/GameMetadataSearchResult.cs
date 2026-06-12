namespace LabubuLog.Services.GameMetadata;

public record GameMetadataSearchResult(
    string Provider,
    string ProviderId,
    string Title,
    string? ThumbnailUrl,
    string? Platforms);
