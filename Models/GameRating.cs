using System.ComponentModel.DataAnnotations;

namespace LabubuLog.Models;

public class GameRating
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public Game Game { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Display(Name = "Score")]
    [Range(1, 10)]
    public int? Score { get; set; }

    [Display(Name = "Favorite moment")]
    [StringLength(220)]
    public string? FavoriteMoment { get; set; }

    [Display(Name = "Rating notes")]
    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime RatedAtUtc { get; set; } = DateTime.UtcNow;
}
