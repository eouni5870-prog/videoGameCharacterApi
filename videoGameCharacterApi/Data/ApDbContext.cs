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
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(e =>
        {
            e.Property(g => g.Name).HasMaxLength(100).IsRequired();
            e.Property(g => g.Genre).HasMaxLength(50).IsRequired();
            e.HasIndex(g => g.Name).IsUnique();
            e.Property(g => g.Price).HasPrecision(10, 2);
            e.Property(g => g.CoverUrl).HasMaxLength(500);
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
            e.Property(u => u.Role).HasMaxLength(20).IsRequired().HasDefaultValue(Roles.User);
        });

        modelBuilder.Entity<Favorite>(e =>
        {
            // A user can like a character only once: the pair is the primary key.
            e.HasKey(f => new { f.UserId, f.CharacterId });

            e.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.Character).WithMany(c => c.Favorites).HasForeignKey(f => f.CharacterId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.Property(r => r.Comment).HasMaxLength(500);

            // One review per user and per character (saving again updates it).
            e.HasIndex(r => new { r.UserId, r.CharacterId }).IsUnique();

            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Character).WithMany(c => c.Reviews).HasForeignKey(r => r.CharacterId).OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.Total).HasPrecision(10, 2);
            e.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(10, 2);
            e.Property(i => i.GameName).HasMaxLength(100).IsRequired();
            // If a game is deleted, past orders keep its name and price (GameId becomes null).
            e.HasOne(i => i.Game).WithMany().HasForeignKey(i => i.GameId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
