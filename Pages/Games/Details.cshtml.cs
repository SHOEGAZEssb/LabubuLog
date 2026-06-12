using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages.Games;

public class DetailsModel(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : PageModel
{
    public Game Game { get; private set; } = null!;

    public IReadOnlyList<ScoreCard> Scores { get; private set; } = [];

    public double? SharedScore { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var game = await dbContext.Games
            .Include(game => game.Ratings)
            .ThenInclude(rating => rating.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(game => game.Id == id);

        if (game is null)
        {
            return NotFound();
        }

        Game = game;
        SharedScore = game.SharedRating;

        var users = await userManager.Users
            .AsNoTracking()
            .ToListAsync();

        Scores = users
            .OrderBy(user => user.UserName == "lebubu" ? 0 : user.UserName == "labubu" ? 1 : 2)
            .ThenBy(user => user.DisplayName)
            .Select(user =>
            {
                var rating = game.Ratings.FirstOrDefault(rating => rating.UserId == user.Id);
                return new ScoreCard(
                    user.ScoreLabel,
                    user.DisplayName,
                    rating?.Score,
                    rating?.FavoriteMoment,
                    rating?.Notes);
            })
            .ToList();

        return Page();
    }

    public record ScoreCard(
        string ScoreLabel,
        string DisplayName,
        int? Score,
        string? FavoriteMoment,
        string? Notes);
}
