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

    [Display(Name = "First played")]
    [DataType(DataType.Date)]
    public DateTime? FirstPlayedOn { get; set; }

    [Display(Name = "Last played")]
    [DataType(DataType.Date)]
    public DateTime? LastPlayedOn { get; set; }

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

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public double? SharedRating
    {
        get
        {
            var ratings = new[] { YourRating, PartnerRating }
                .Where(rating => rating.HasValue)
                .Select(rating => rating!.Value)
                .ToList();

            return ratings.Count == 0 ? null : Math.Round(ratings.Average(), 1);
        }
    }
}
