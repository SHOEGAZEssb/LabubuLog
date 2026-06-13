using System.Text.Json;

namespace LabubuLog.Services.GameMetadata;

public class CompositeGameMetadataProvider(IEnumerable<IGameMetadataProvider> providers) : IGameMetadataProvider
{
    private const char ProviderSeparator = ':';
    private readonly IReadOnlyList<IGameMetadataProvider> providers = providers.ToList();

    public string Name => "Steam + RAWG";

    public async Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var providerTasks = providers.Select(provider => SearchProviderAsync(provider, searchTerm, cancellationToken));
        var providerResults = await Task.WhenAll(providerTasks);

        return providerResults
            .SelectMany(results => results)
            .Take(12)
            .ToList();
    }

    public async Task<GameMetadataDetails?> GetDetailsAsync(string providerId, CancellationToken cancellationToken)
    {
        var separatorIndex = providerId.IndexOf(ProviderSeparator);

        if (separatorIndex <= 0 || separatorIndex == providerId.Length - 1)
        {
            return null;
        }

        var providerName = providerId[..separatorIndex];
        var sourceProviderId = providerId[(separatorIndex + 1)..];
        var provider = providers.FirstOrDefault(provider =>
            string.Equals(provider.Name, providerName, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            return null;
        }

        return await provider.GetDetailsAsync(sourceProviderId, cancellationToken);
    }

    private static async Task<IReadOnlyList<GameMetadataSearchResult>> SearchProviderAsync(
        IGameMetadataProvider provider,
        string searchTerm,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await provider.SearchAsync(searchTerm, cancellationToken);

            return results
                .Select(result => result with
                {
                    ProviderId = $"{provider.Name}{ProviderSeparator}{result.ProviderId}"
                })
                .ToList();
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
