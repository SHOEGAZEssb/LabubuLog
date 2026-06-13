using System.ComponentModel.DataAnnotations;

namespace LabubuLog.Models;

public class Game
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Platform { get; set; }

    [StringLength(80)]
    public string? Genre { get; set; }

    [Display(Name = "Release year")]
    [Range(1970, 2100)]
    public int? ReleaseYear { get; set; }

    [Display(Name = "Cover image URL")]
    [StringLength(500)]
    [Url]
    public string? CoverImageUrl { get; set; }

    [Display(Name = "Horizontal title image URL")]
    [StringLength(500)]
    [Url]
    public string? TitleImageUrl { get; set; }

    [StringLength(160)]
    public string? Tags { get; set; }

    public PlayStatus Status { get; set; } = PlayStatus.Playing;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<GameRating> Ratings { get; set; } = [];

    public double? SharedRating
    {
        get
        {
            var ratings = Ratings
                .Where(rating => rating.Score.HasValue)
                .Select(rating => rating.Score!.Value)
                .ToList();

            return ratings.Count == 0 ? null : Math.Round(ratings.Average(), 1);
        }
    }
}
