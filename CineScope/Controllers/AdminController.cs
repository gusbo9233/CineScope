using CineScope.Models;
using CineScope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineScope.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMovieService _movieService;

        public AdminController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            var orderedMovies = movies.OrderBy(movie => movie.Title).ToList();

            var overallAvg = await _movieService.GetOverallAverageRatingAsync();
            var totalRatings = await _movieService.GetTotalRatingCountAsync();
            ViewData["TotalRatingCount"] = totalRatings;

            var viewModel = new AdminDashboardViewModel
            {
                Movies = orderedMovies,
                MovieCount = orderedMovies.Count,
                AverageRating = overallAvg ?? 0m,
                TotalRuntime = orderedMovies.Sum(movie => movie.Duration)
            };

            return View(viewModel);
        }


    }
}
