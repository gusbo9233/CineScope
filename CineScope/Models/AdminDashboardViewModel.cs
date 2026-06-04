namespace CineScope.Models
{
    public class AdminDashboardViewModel
    {
        public IList<MovieModel> Movies { get; set; } = new List<MovieModel>();

        public int MovieCount { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalRuntime { get; set; }
    }
}
