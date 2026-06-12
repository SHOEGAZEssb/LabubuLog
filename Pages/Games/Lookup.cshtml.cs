using System.Text.Json;
using LabubuLog.Services.GameMetadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabubuLog.Pages.Games;

public class LookupModel(IGameMetadataProvider metadataProvider) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public IReadOnlyList<GameMetadataSearchResult> Results { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            return;
        }

        try
        {
            Results = await metadataProvider.SearchAsync(SearchTerm, cancellationToken);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "The metadata provider could not be reached.";
        }
        catch (JsonException)
        {
            ErrorMessage = "The metadata provider returned an unexpected response.";
        }
    }

    public async Task<IActionResult> OnPostUseAsync(string providerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            return RedirectToPage("Lookup", new { SearchTerm });
        }

        try
        {
            var details = await metadataProvider.GetDetailsAsync(providerId, cancellationToken);

            if (details is null)
            {
                TempData["LookupError"] = "That result could not be imported.";
                return RedirectToPage("Lookup", new { SearchTerm });
            }

            TempData["GameDraft"] = JsonSerializer.Serialize(details);
            return RedirectToPage("Create");
        }
        catch (HttpRequestException)
        {
            TempData["LookupError"] = "The metadata provider could not be reached.";
            return RedirectToPage("Lookup", new { SearchTerm });
        }
        catch (JsonException)
        {
            TempData["LookupError"] = "The metadata provider returned an unexpected response.";
            return RedirectToPage("Lookup", new { SearchTerm });
        }
    }
}
