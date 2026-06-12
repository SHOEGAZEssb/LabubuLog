using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LabubuLog.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(40)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string ScoreLabel { get; set; } = string.Empty;

    public List<GameRating> GameRatings { get; set; } = [];
}
