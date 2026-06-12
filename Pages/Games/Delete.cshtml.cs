using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages.Games;

public class DeleteModel(ApplicationDbContext dbContext) : PageModel
{
    public Game Game { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var game = await dbContext.Games.AsNoTracking().FirstOrDefaultAsync(game => game.Id == id);

        if (game is null)
        {
            return NotFound();
        }

        Game = game;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var game = await dbContext.Games.FindAsync(id);

        if (game is null)
        {
            return RedirectToPage("Index");
        }

        dbContext.Games.Remove(game);
        await dbContext.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
