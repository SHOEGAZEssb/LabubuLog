using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages;

public class IndexModel(ApplicationDbContext dbContext) : PageModel
{
    public int TotalGames { get; private set; }

    public int CompletedGames { get; private set; }

    public int RatedGames { get; private set; }

    public double? AverageSharedRating { get; private set; }

    public IReadOnlyList<Game> RecentGames { get; private set; } = [];

    public IReadOnlyList<Game> TopGames { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var games = await dbContext.Games
            .Include(game => game.Ratings)
            .AsNoTracking()
            .ToListAsync();

        TotalGames = games.Count;
        CompletedGames = games.Count(game => game.Status == PlayStatus.Completed);
        RatedGames = games.Count(game => game.SharedRating.HasValue);

        var sharedRatings = games
            .Select(game => game.SharedRating)
            .Where(rating => rating.HasValue)
            .Select(rating => rating!.Value)
            .ToList();

        AverageSharedRating = sharedRatings.Count == 0 ? null : Math.Round(sharedRatings.Average(), 1);

        RecentGames = games
            .OrderByDescending(game => game.LastPlayedOn ?? game.FirstPlayedOn ?? game.CreatedAtUtc)
            .Take(5)
            .ToList();

        TopGames = games
            .Where(game => game.SharedRating.HasValue)
            .OrderByDescending(game => game.SharedRating)
            .ThenBy(game => game.Title)
            .Take(5)
            .ToList();
    }
}
