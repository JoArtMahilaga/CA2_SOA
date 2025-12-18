using CA2SOA.Entities;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Data;

public sealed class GameShelfDbContext : DbContext
{
    public GameShelfDbContext(DbContextOptions<GameShelfDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<LibraryEntry> LibraryEntries => Set<LibraryEntry>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Genre>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(80);
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Game>(b =>
        {
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Platform).IsRequired().HasMaxLength(80);

            b.HasOne(x => x.Genre)
                .WithMany(g => g.Games)
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(x => x.UserName).IsUnique();
            b.Property(x => x.UserName).IsRequired().HasMaxLength(64);
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.PasswordSalt).IsRequired();
        });

        modelBuilder.Entity<LibraryEntry>(b =>
        {
            b.HasOne(x => x.User)
                .WithMany(u => u.LibraryEntries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Game)
                .WithMany(g => g.LibraryEntries)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.UserId, x.GameId }).IsUnique();
        });

        modelBuilder.Entity<Review>(b =>
        {
            b.Property(x => x.Rating).IsRequired();
            b.Property(x => x.Comment).HasMaxLength(1000);

            b.HasOne(x => x.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // one review per user per game
            b.HasIndex(x => new { x.UserId, x.GameId }).IsUnique();
        });
    }
}
