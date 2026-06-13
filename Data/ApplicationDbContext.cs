using LabubuLog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Game> Games => Set<Game>();

    public DbSet<GameRating> GameRatings => Set<GameRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var titleProperty = modelBuilder.Entity<Game>()
            .Property(game => game.Title);

        if (Database.IsSqlite())
        {
            titleProperty.UseCollation("NOCASE");
        }

        modelBuilder.Entity<GameRating>()
            .HasIndex(rating => new { rating.GameId, rating.UserId })
            .IsUnique();

        modelBuilder.Entity<GameRating>()
            .HasOne(rating => rating.Game)
            .WithMany(game => game.Ratings)
            .HasForeignKey(rating => rating.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameRating>()
            .HasOne(rating => rating.User)
            .WithMany(user => user.GameRatings)
            .HasForeignKey(rating => rating.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
