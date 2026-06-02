using CineScope.Data;
using CineScope.Models;
using System.Net.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
namespace CineScope.Services
{
    public class MovieService : IMovieService
    {
        private readonly MovieContext _context;
        private readonly HttpClient _httpClient;
        private const string OMDB_API_BASE_URL = "https://www.omdbapi.com/";
        private const string OMDb_API_KEY = "97fdc569"; 

        public MovieService(MovieContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<List<MovieModel>> FetchAndCreatePopularMoviesAsync()
        {
            var imdbIds = new[]
                {
            "tt0111161", // The Shawshank Redemption
            "tt0068646", // The Godfather
            "tt0468569", // The Dark Knight
            "tt0071562", // The Godfather Part II
            "tt0050083", // 12 Angry Men
            "tt0108052", // Schindler's List
            "tt0167260", // The Lord of the Rings: The Return of the King
            "tt0110912", // Pulp Fiction
            "tt0120737", // The Lord of the Rings: The Fellowship of the Ring
            "tt0060196"  // The Good, the Bad and the Ugly
            };

            var movies = new List<MovieModel>();

            foreach (var imdbId in imdbIds)
            {
                try
                {
                    var movie = await FetchMovieFromApiAsync(imdbId);
                    var existingMovie = await _context.Movies.FirstOrDefaultAsync(m => m.Title == movie.Title);
                    
                    if (existingMovie == null)
                    {
                        await CreateMovieAsync(movie);
                        movies.Add(movie);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return movies;
        }

        public async Task<List<MovieModel>> SearchMoviesFromApiAsync(string searchQuery)
        {
            try
            {
                var url = $"{OMDB_API_BASE_URL}?s={Uri.EscapeDataString(searchQuery)}&apikey={OMDb_API_KEY}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var searchResponse = JsonSerializer.Deserialize<MovieSearchResponse>(content, options);

                if (searchResponse?.Response != "True" || searchResponse.Search == null)
                {
                    return new List<MovieModel>();
                }

                var movies = new List<MovieModel>();
                foreach (var result in searchResponse.Search)
                {
                    try
                    {
                        var fullMovie = await FetchMovieFromApiAsync(result.ImdbID);
                        movies.Add(fullMovie);
                    }
                    catch
                    {
                        // Skip movies that fail to fetch full details
                    }
                }

                return movies;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching movies: {ex.Message}");
            }
        }

        public async Task<MovieModel> FetchMovieFromApiAsync(string imdbId)
        {
            try
            {
                var url = $"{OMDB_API_BASE_URL}?i={imdbId}&apikey={OMDb_API_KEY}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiMovie = JsonSerializer.Deserialize<MovieApiResponse>(content, options);

                if (apiMovie?.Response != "True")
                {
                    throw new Exception("Movie not found in API");
                }

                var movie = MapApiResponseToMovie(apiMovie);
                return movie;
            }

            catch (Exception ex)
            {
                throw new Exception($"Error fetching movie from API: {ex.Message}");
            }
        }   

        public async Task<MovieModel> FetchAndCreateMovieFromApiAsync(string imdbId)
        {
            var movie = await FetchMovieFromApiAsync(imdbId);
            await CreateMovieAsync(movie);
            return movie;
        }

        private MovieModel MapApiResponseToMovie(MovieApiResponse apiMovie)
        {
            return new MovieModel
            {
                Title = apiMovie.Title,
                Genre = apiMovie.Genre,
                ReleaseYear = int.TryParse(apiMovie.Year, out int year) ? year : DateTime.Now.Year,
                Rating = decimal.TryParse(apiMovie.imdbRating, out decimal rating) ? rating : 0,
                Duration = ExtractDurationInMinutes(apiMovie.Runtime),
                PosterUrl = apiMovie.Poster,
                Description = apiMovie.Plot,
                ImdbId = apiMovie.ImdbID
            };
        }

        private int ExtractDurationInMinutes(string runtime)
        {
            // Extract number from "136 min"
            var parts = runtime?.Split(' ');
            return int.TryParse(parts?[0], out int minutes) ? minutes : 0;
        }

        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            return await _context.Movies.ToListAsync();
        }

        public async Task<MovieModel?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);

        }

        public async Task<MovieModel> CreateMovieAsync(MovieModel movie)
        {
            _context.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<MovieModel> UpdateMovieAsync(MovieModel movie)
        {
            _context.Update(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task DeleteMovieAsync(int id) {
            var movie = await GetMovieByIdAsync(id);
            if (movie != null) {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> DeleteMoviesAsync(IEnumerable<int> ids)
        {
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0)
            {
                return 0;
            }

            var movies = await _context.Movies
                .Where(movie => idList.Contains(movie.Id))
                .ToListAsync();

            _context.Movies.RemoveRange(movies);
            await _context.SaveChangesAsync();

            return movies.Count;
        }
        
        public bool MovieExists(int id)
        {
            return _context.Movies.Any(m => m.Id == id);
        }

    }
}
