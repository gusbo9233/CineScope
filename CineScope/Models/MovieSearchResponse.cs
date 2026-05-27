namespace CineScope.Models
{
    public class MovieSearchResponse
    {
        public string Response { get; set; } = string.Empty;
        public List<MovieSearchResult> Search { get; set; } = new();
        public string TotalResults { get; set; } = string.Empty;
    }

    public class MovieSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string ImdbID { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
    }
}