using System.ComponentModel.DataAnnotations;

namespace CineScope.Models
{
    public class AdminDashboardViewModel
    {
        public IList<MovieModel> Movies { get; set; } = new List<MovieModel>();

        public int MovieCount { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalRuntime { get; set; }
    }

    public class AdminMovieInput
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Genre { get; set; } = string.Empty;

        [Range(1888, 2100)]
        public int ReleaseYear { get; set; }

        [Range(0, 10)]
        public decimal Rating { get; set; }

        [Range(0, int.MaxValue)]
        public int Duration { get; set; }

        public string PosterUrl { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImdbId { get; set; } = string.Empty;
    }
}
