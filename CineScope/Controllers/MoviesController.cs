using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CineScope.Models;
using CineScope.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CineScope.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> SeedPopularMovies()
        {
            try
            {
                var movies = await _movieService.FetchAndCreatePopularMoviesAsync();
                TempData["SuccessMessage"] = $"Successfully loaded {movies.Count} popular movies!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                var allMovies = await _movieService.GetAllMoviesAsync();
                return View("Index", allMovies);
            }
        }
        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            var overallAvg = await _movieService.GetOverallAverageRatingAsync();
            var totalRatings = await _movieService.GetTotalRatingCountAsync();
            ViewData["OverallAverageRating"] = overallAvg;
            ViewData["TotalRatingCount"] = totalRatings;
            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieService.GetMovieByIdAsync(id.Value);
            if (movie == null)
            {
                return NotFound();
            }

            // compute average + user rating for the view
            var avg = await _movieService.GetAverageRatingAsync(movie.Id);
            var ratingCount = await _movieService.GetRatingCountAsync(movie.Id);
            int? userRating = null;
            bool isFavorite = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    userRating = await _movieService.GetUserRatingAsync(movie.Id, userId);
                    isFavorite = await _movieService.IsFavoriteAsync(movie.Id, userId);
                }
            }

            // Load comments and attach to the model so the view can render them
            var comments = await _movieService.GetCommentsAsync(movie.Id);
            movie.Comments = comments ?? new List<CineScope.Models.MovieComment>();

            // only supply AverageRating when there are ratings and average is non-zero; treat 0 as unrated
            ViewData["AverageRating"] = (ratingCount == 0 || avg == 0m) ? null : (decimal?)avg;
            ViewData["RatingCount"] = ratingCount;
            ViewData["UserRating"] = userRating;
            ViewData["IsFavorite"] = isFavorite;

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int id, int stars)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                // send user to sign-in (app uses Microsoft Identity sign-in path elsewhere)
                return Redirect($"/MicrosoftIdentity/Account/SignIn?returnUrl=/Movies/Details/{id}");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Unable to determine your user id.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _movieService.AddOrUpdateRatingAsync(id, userId, stars);
                TempData["SuccessMessage"] = "Your rating has been saved.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not save rating: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Movies/Edit/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieService.GetMovieByIdAsync(id.Value);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,ReleaseYear,Rating,Duration,PosterUrl,Description")] MovieModel movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _movieService.UpdateMovieAsync(movie);
                    TempData["SuccesMessage"] = "Movie updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = movie.Id });
                }

                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating movie: {ex.Message}");
                }

            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieService.GetMovieByIdAsync(id.Value);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _movieService.DeleteMovieAsync(id);
                TempData["SuccessMessage"] = "Movie deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting movie: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }


        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchMovies(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                ModelState.AddModelError("searchQuery", "Please enter a search term");
                return View("Search");
            }

            try
            {
                var results = await _movieService.SearchMoviesFromApiAsync(searchQuery);
                ViewData["SearchQuery"] = searchQuery;
                return View("SearchResults", results);
            }

            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Search error: {ex.Message}");
                return View("Search");
            }
        }

        // POST: Movies/ImportMovie
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ImportMovie(string imdbId)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Redirect("/MicrosoftIdentity/Account/SignIn?returnUrl=%2FMovies%2FSearch");
            }

            if (!User.IsInRole("Admin"))
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            try
            {
                var movie = await _movieService.FetchMovieFromApiAsync(imdbId);
                var allMovies = await _movieService.GetAllMoviesAsync();

                if (allMovies.Any(m => m.Title == movie.Title))
                {
                    TempData["InfoMessage"] = $"'{movie.Title}' already exists in your database.";
                    return RedirectToAction(nameof(Search));
                }

                await _movieService.CreateMovieAsync(movie);
                TempData["SuccessMessage"] = $"'{movie.Title}' successfully imported!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Import failed: {ex.Message}";
                return RedirectToAction(nameof(Search));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Comments(int id)
        {
            var comments = await _movieService.GetCommentsAsync(id);
            return PartialView("_Comments", comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int movieId, string content)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Redirect($"/MicrosoftIdentity/Account/SignIn?returnUrl=/Movies/Details/{movieId}");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Comment content cannot be empty";
                return RedirectToAction(nameof(Details), new { id = movieId });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Unable to determine your user id.";
                return RedirectToAction(nameof(Details), new { id = movieId });
            }

            try
            {
                await _movieService.AddCommentAsync(movieId, userId, userName, content);
                TempData["SuccessMessage"] = "Your comment has been posted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not save comment: {ex.Message}";
            }

            // If AJAX request, return updated comments partial to avoid full page reload
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var comments = await _movieService.GetCommentsAsync(movieId);
                return PartialView("_Comments", comments);
            }

            return RedirectToAction(nameof(Details), new { id = movieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Unable to determine user.");
            }

            try
            {
                var currently = await _movieService.IsFavoriteAsync(id, userId);
                if (currently)
                {
                    await _movieService.RemoveFavoriteAsync(id, userId);
                }
                else
                {
                    await _movieService.AddFavoriteAsync(id, userId);
                }

                return Json(new { favorited = !currently });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // or Redirect to sign-in
            }

            var favorites = await _movieService.GetFavoritesAsync(userId);
            // Use explicit view path to avoid runtime lookup failures
            return View("~/Views/Movies/Favorites.cshtml", favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            try
            {
                await _movieService.RemoveFavoriteAsync(id, userId);
                TempData["SuccessMessage"] = "Removed from favorites.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not remove favorite: {ex.Message}";
            }

            return RedirectToAction(nameof(Favorites));
        }

    }
}
