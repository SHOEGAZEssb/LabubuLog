using System.ComponentModel.DataAnnotations;
using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages.Games;

public class RateModel(ApplicationDbContext dbContext) : PageModel
{
    public Game Game { get; private set; } = null!;

    [BindProperty]
    public RatingInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var game = await dbContext.Games.AsNoTracking().FirstOrDefaultAsync(game => game.Id == id);

        if (game is null)
        {
            return NotFound();
        }

        Game = game;
        Input = RatingInput.FromGame(game);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var game = await dbContext.Games.FirstOrDefaultAsync(game => game.Id == id);

        if (game is null)
        {
            return NotFound();
        }

        Game = game;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        game.YourRating = Input.YourRating;
        game.PartnerRating = Input.PartnerRating;
        game.FavoriteMoment = Input.FavoriteMoment;
        game.RatingNotes = Input.RatingNotes;

        await dbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { id = game.Id });
    }

    public class RatingInput
    {
        [Display(Name = "Lebubu Score")]
        [Range(1, 10)]
        public int? YourRating { get; set; }

        [Display(Name = "Labubu Score")]
        [Range(1, 10)]
        public int? PartnerRating { get; set; }

        [Display(Name = "Favorite moment")]
        [StringLength(220)]
        public string? FavoriteMoment { get; set; }

        [Display(Name = "Rating notes")]
        [StringLength(1000)]
        public string? RatingNotes { get; set; }

        public static RatingInput FromGame(Game game)
        {
            return new RatingInput
            {
                YourRating = game.YourRating,
                PartnerRating = game.PartnerRating,
                FavoriteMoment = game.FavoriteMoment,
                RatingNotes = game.RatingNotes
            };
        }
    }
}
