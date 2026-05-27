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
        bool MovieExists(int id);
        Task<MovieModel> FetchMovieFromApiAsync(string imdbId);
        Task<MovieModel> FetchAndCreateMovieFromApiAsync(string imdbId);
        Task<List<MovieModel>> FetchAndCreatePopularMoviesAsync();

        Task<List<MovieModel>> SearchMoviesFromApiAsync(string searchQuery);

    }
}
