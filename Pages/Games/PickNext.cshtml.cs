using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages.Games;

public class PickNextModel(ApplicationDbContext dbContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? PlannedSeed { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PausedSeed { get; set; }

    public int NextPlannedSeed { get; private set; }

    public int NextPausedSeed { get; private set; }

    public IReadOnlyList<PickBucket> Picks { get; private set; } = [];

    public async Task OnGetAsync()
    {
        PlannedSeed = NormalizeSeed(PlannedSeed);
        PausedSeed = NormalizeSeed(PausedSeed);
        NextPlannedSeed = NewSeedExcept(PlannedSeed.Value);
        NextPausedSeed = NewSeedExcept(PausedSeed.Value);

        var candidates = await dbContext.Games
            .Include(game => game.Ratings)
            .Where(game => game.Status == PlayStatus.Planned || game.Status == PlayStatus.Paused)
            .AsNoTracking()
            .ToListAsync();

        Picks =
        [
            BuildPick(PlayStatus.Planned, "Planned pick", PlannedSeed.Value, candidates),
            BuildPick(PlayStatus.Paused, "Paused pick", PausedSeed.Value, candidates)
        ];
    }

    private static PickBucket BuildPick(PlayStatus status, string title, int seed, IReadOnlyList<Game> candidates)
    {
        var games = candidates
            .Where(game => game.Status == status)
            .OrderBy(game => game.Title)
            .ToList();

        var game = games.Count == 0 ? null : games[new Random(seed).Next(games.Count)];

        return new PickBucket(
            status,
            title,
            games.Count,
            game);
    }

    private static int NormalizeSeed(int? seed) =>
        seed is > 0 ? seed.Value : NewSeed();

    private static int NewSeedExcept(int currentSeed)
    {
        var seed = NewSeed();
        return seed == currentSeed ? NewSeed() : seed;
    }

    private static int NewSeed() =>
        Random.Shared.Next(1, int.MaxValue);

    public record PickBucket(
        PlayStatus Status,
        string Title,
        int TotalCount,
        Game? Game);
}
