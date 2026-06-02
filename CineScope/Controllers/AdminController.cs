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

            var viewModel = new AdminDashboardViewModel
            {
                Movies = orderedMovies,
                MovieCount = orderedMovies.Count,
                AverageRating = orderedMovies.Count == 0 ? 0 : Math.Round(orderedMovies.Average(movie => movie.Rating), 1),
                TotalRuntime = orderedMovies.Sum(movie => movie.Duration)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMovie(AdminMovieInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Movie update failed. Check the values and try again.";
                return RedirectToAction(nameof(Dashboard));
            }

            var movie = new MovieModel
            {
                Id = input.Id,
                Title = input.Title.Trim(),
                Genre = input.Genre.Trim(),
                ReleaseYear = input.ReleaseYear,
                Rating = input.Rating,
                Duration = input.Duration,
                PosterUrl = input.PosterUrl.Trim(),
                Description = input.Description.Trim(),
                ImdbId = input.ImdbId.Trim()
            };

            await _movieService.UpdateMovieAsync(movie);
            TempData["SuccessMessage"] = $"Saved changes to \"{movie.Title}\".";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int[] selectedMovieIds)
        {
            if (selectedMovieIds.Length == 0)
            {
                TempData["ErrorMessage"] = "Select at least one movie to delete.";
                return RedirectToAction(nameof(Dashboard));
            }

            var deletedCount = await _movieService.DeleteMoviesAsync(selectedMovieIds);
            TempData["SuccessMessage"] = $"Deleted {deletedCount} movie{(deletedCount == 1 ? string.Empty : "s")}.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
