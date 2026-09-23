using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Character> Characters { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(e =>
        {
            e.Property(g => g.Name).HasMaxLength(100).IsRequired();
            e.Property(g => g.Genre).HasMaxLength(50).IsRequired();
            e.HasIndex(g => g.Name).IsUnique();
        });

        modelBuilder.Entity<Character>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Role).HasMaxLength(50).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000);
            e.Property(c => c.ImageUrl).HasMaxLength(500);

            // One game has many characters. A game cannot be deleted while it still has characters.
            e.HasOne(c => c.Game)
             .WithMany(g => g.Characters)
             .HasForeignKey(c => c.GameId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
        });

    }
}
