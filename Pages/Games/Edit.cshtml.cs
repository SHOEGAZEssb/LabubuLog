using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabubuLog.Pages.Games;

public class EditModel(ApplicationDbContext dbContext) : PageModel
{
    [BindProperty]
    public Game Game { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var game = await dbContext.Games.FindAsync(id);

        if (game is null)
        {
            return NotFound();
        }

        Game = game;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var game = await dbContext.Games.FindAsync(Game.Id);

        if (game is null)
        {
            return NotFound();
        }

        game.Title = Game.Title;
        game.Platform = Game.Platform;
        game.Genre = Game.Genre;
        game.ReleaseYear = Game.ReleaseYear;
        game.CoverImageUrl = Game.CoverImageUrl;
        game.TitleImageUrl = Game.TitleImageUrl;
        game.Tags = Game.Tags;
        game.Status = Game.Status;

        await dbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { id = Game.Id });
    }
}
