using LabubuLog.Data;
using LabubuLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Pages.Games;

public class IndexModel(ApplicationDbContext dbContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public PlayStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "recent";

    public IReadOnlyList<Game> Games { get; private set; } = [];

    public SelectList StatusOptions { get; private set; } = new(Enum.GetValues<PlayStatus>());

    public static IEnumerable<GameRating> RatingsWithText(Game game) =>
        game.Ratings.Where(rating =>
            !string.IsNullOrWhiteSpace(rating.FavoriteMoment) ||
            !string.IsNullOrWhiteSpace(rating.Notes));

    public async Task OnGetAsync()
    {
        var query = dbContext.Games
            .Include(game => game.Ratings)
            .ThenInclude(rating => rating.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim();
            query = query.Where(game =>
                game.Title.Contains(term) ||
                (game.Platform != null && game.Platform.Contains(term)) ||
                (game.Genre != null && game.Genre.Contains(term)) ||
                (game.Tags != null && game.Tags.Contains(term)));
        }

        if (StatusFilter.HasValue)
        {
            query = query.Where(game => game.Status == StatusFilter);
        }

        var games = await query.ToListAsync();

        Games = SortBy switch
        {
            "title" => games.OrderBy(game => game.Title).ToList(),
            "rating" => games.OrderByDescending(game => game.SharedRating ?? 0).ThenBy(game => game.Title).ToList(),
            "status" => games.OrderBy(game => game.Status).ThenBy(game => game.Title).ToList(),
            _ => games.OrderByDescending(game => game.CreatedAtUtc).ToList()
        };
    }
}
