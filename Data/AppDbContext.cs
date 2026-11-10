using AnimeWiki.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeWiki.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Anime> Anime => Set<Anime>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.NormalizedUsername).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.HasIndex(u => u.NormalizedUsername).IsUnique();
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<Anime>()
            .HasIndex(a => a.AniListId)
            .IsUnique();

        modelBuilder.Entity<Genre>()
            .HasIndex(g => g.Name)
            .IsUnique();

        modelBuilder.Entity<Character>()
            .HasIndex(c => c.AniListId)
            .IsUnique();

        modelBuilder.Entity<Anime>()
            .HasMany(a => a.Genres)
            .WithMany(g => g.Animes)
            .UsingEntity(j => j.ToTable("AnimeGenre"));

        modelBuilder.Entity<Anime>()
            .HasMany(a => a.Characters)
            .WithMany(c => c.Animes)
            .UsingEntity(j => j.ToTable("AnimeCharacter"));
    }
}
