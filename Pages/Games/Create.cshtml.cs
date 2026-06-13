using System.Text.Json;
using LabubuLog.Data;
using LabubuLog.Models;
using LabubuLog.Services.GameMetadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabubuLog.Pages.Games;

public class CreateModel(ApplicationDbContext dbContext) : PageModel
{
    [BindProperty]
    public Game Game { get; set; } = new();

    public void OnGet()
    {
        var draft = ReadMetadataDraft();

        if (draft is null)
        {
            return;
        }

        Game = new Game
        {
            Title = draft.Title,
            Platform = draft.Platform,
            Genre = draft.Genre,
            ReleaseYear = draft.ReleaseYear,
            CoverImageUrl = draft.CoverImageUrl,
            TitleImageUrl = draft.TitleImageUrl,
            Tags = draft.Tags,
            Status = PlayStatus.Playing
        };

        TempData["LookupApplied"] = $"Metadata imported from {draft.Provider}.";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Game.CreatedAtUtc = DateTime.UtcNow;

        dbContext.Games.Add(Game);
        await dbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { id = Game.Id });
    }

    private GameMetadataDetails? ReadMetadataDraft()
    {
        if (TempData["GameDraft"] is not string draftJson || string.IsNullOrWhiteSpace(draftJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GameMetadataDetails>(draftJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
