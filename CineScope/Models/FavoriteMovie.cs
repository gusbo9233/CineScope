namespace CineScope.Models
{
    public class FavoriteMovie
    {
        public int MovieId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
