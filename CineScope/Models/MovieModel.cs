namespace CineScope.Models
{
    public class MovieModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public decimal Rating { get; set; }
        public int Duration { get; set; }
        public string PosterUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImdbId { get; set; } = string.Empty;
        public List<MovieComment> Comments { get; set; } = new();
    }
}
