namespace LabubuLog.Services.GameMetadata;

public interface IGameMetadataProvider
{
    string Name { get; }

    Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken);

    Task<GameMetadataDetails?> GetDetailsAsync(string providerId, CancellationToken cancellationToken);
}
