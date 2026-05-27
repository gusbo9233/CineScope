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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieModel>()
                .Property(movie => movie.Rating)
                .HasPrecision(3, 1);
        }
    }
}
