namespace CineScope.Models
{
    public class MovieRating
    {
        public int MovieId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Stars { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
