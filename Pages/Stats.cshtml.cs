using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages;

public class StatsModel(ApplicationDbContext dbContext) : PageModel
{
    public int TotalGames { get; private set; }

    public int CompletedGames { get; private set; }

    public int RatedGames { get; private set; }

    public double? AverageSharedRating { get; private set; }

    public IReadOnlyList<Game> TopRatedGames { get; private set; } = [];

    public IReadOnlyDictionary<PlayStatus, int> StatusCounts { get; private set; } = new Dictionary<PlayStatus, int>();

    public async Task OnGetAsync()
    {
        var games = await dbContext.Games.AsNoTracking().ToListAsync();

        TotalGames = games.Count;
        CompletedGames = games.Count(game => game.Status == PlayStatus.Completed);
        RatedGames = games.Count(game => game.SharedRating.HasValue);

        var sharedRatings = games
            .Select(game => game.SharedRating)
            .Where(rating => rating.HasValue)
            .Select(rating => rating!.Value)
            .ToList();

        AverageSharedRating = sharedRatings.Count == 0 ? null : Math.Round(sharedRatings.Average(), 1);

        TopRatedGames = games
            .Where(game => game.SharedRating.HasValue)
            .OrderByDescending(game => game.SharedRating ?? 0)
            .ThenBy(game => game.Title)
            .Take(6)
            .ToList();

        StatusCounts = games
            .GroupBy(game => game.Status)
            .ToDictionary(group => group.Key, group => group.Count());
    }
}
