using CineScope.Models;

namespace CineScope.Services
{
    public interface IMovieService
    {
        Task<List<MovieModel>> GetAllMoviesAsync();
        Task<MovieModel?> GetMovieByIdAsync(int id);
        Task<MovieModel> CreateMovieAsync(MovieModel movie);
        Task<MovieModel> UpdateMovieAsync(MovieModel movie);
        Task DeleteMovieAsync(int id);
        Task<int> DeleteMoviesAsync(IEnumerable<int> ids);
        bool MovieExists(int id);
        Task<MovieModel> FetchMovieFromApiAsync(string imdbId);
        Task<MovieModel> FetchAndCreateMovieFromApiAsync(string imdbId);
        Task<List<MovieModel>> FetchAndCreatePopularMoviesAsync();
        Task<List<MovieModel>> SearchMoviesFromApiAsync(string searchQuery);

        // Rating APIs
        Task AddOrUpdateRatingAsync(int movieId, string userId, int stars);
        Task<decimal> GetAverageRatingAsync(int movieId);
        Task<int?> GetUserRatingAsync(int movieId, string userId);

        Task<List<MovieComment>> GetCommentsAsync(int movieId);
        Task AddCommentAsync(int movieId, string userId, string userName, string content);
    }
}
