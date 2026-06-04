using CineScope.Models;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Data
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options)
            : base(options)
        {
        }

        public DbSet<MovieModel> Movies { get; set; }
        public DbSet<MovieRating> Ratings { get; set; }
        public DbSet<MovieComment> Comments { get; set; }
        public DbSet<FavoriteMovie> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieModel>()
                .Property(movie => movie.Rating)
                .HasPrecision(3, 1);

            modelBuilder.Entity<MovieRating>()
                .HasKey(r => new { r.MovieId, r.UserId });

            modelBuilder.Entity<MovieRating>()
                .Property(r => r.Stars)
                .IsRequired();

            modelBuilder.Entity<MovieComment>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<MovieComment>()
                .Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<MovieComment>()
                .HasOne<MovieModel>()
                .WithMany(m => m.Comments)
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteMovie>()
                .HasKey(f => new { f.MovieId, f.UserId });

            modelBuilder.Entity<FavoriteMovie>()
                .HasOne<MovieModel>()
                .WithMany()
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
