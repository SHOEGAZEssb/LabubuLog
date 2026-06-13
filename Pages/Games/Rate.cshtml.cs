using System.ComponentModel.DataAnnotations;
using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages.Games;

public class RateModel(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : PageModel
{
    public Game Game { get; private set; } = null!;

    public string ScoreLabel { get; private set; } = "Score";

    [BindProperty]
    public RatingInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var game = await dbContext.Games.AsNoTracking().FirstOrDefaultAsync(game => game.Id == id);
        var user = await userManager.GetUserAsync(User);

        if (game is null || user is null)
        {
            return NotFound();
        }

        Game = game;
        ScoreLabel = user.ScoreLabel;

        var rating = await dbContext.GameRatings
            .AsNoTracking()
            .FirstOrDefaultAsync(rating => rating.GameId == game.Id && rating.UserId == user.Id);

        Input = RatingInput.FromRating(rating);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var game = await dbContext.Games.FirstOrDefaultAsync(game => game.Id == id);
        var user = await userManager.GetUserAsync(User);

        if (game is null || user is null)
        {
            return NotFound();
        }

        Game = game;
        ScoreLabel = user.ScoreLabel;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var rating = await dbContext.GameRatings
            .FirstOrDefaultAsync(rating => rating.GameId == game.Id && rating.UserId == user.Id);

        if (rating is null)
        {
            rating = new GameRating
            {
                GameId = game.Id,
                UserId = user.Id
            };

            dbContext.GameRatings.Add(rating);
        }

        rating.Score = Input.Score;
        rating.FavoriteMoment = Input.FavoriteMoment;
        rating.Notes = Input.Notes;
        rating.RatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { id = game.Id });
    }

    public class RatingInput
    {
        [Required]
        [Display(Name = "Score")]
        [Range(1, 10)]
        public int? Score { get; set; } = 5;

        [Display(Name = "Favorite moment")]
        [StringLength(220)]
        public string? FavoriteMoment { get; set; }

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        public static RatingInput FromRating(GameRating? rating)
        {
            return new RatingInput
            {
                Score = rating?.Score ?? 5,
                FavoriteMoment = rating?.FavoriteMoment,
                Notes = rating?.Notes
            };
        }
    }
}
