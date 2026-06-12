using LabubuLog.Models;
using Microsoft.EntityFrameworkCore;

namespace LabubuLog.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>()
            .Property(game => game.Title)
            .UseCollation("NOCASE");

    }
}
