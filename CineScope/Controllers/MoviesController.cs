using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CineScope.Data;
using CineScope.Models;
using CineScope.Services;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

            return View(movie);
        }

        // GET: Movies/Edit/5
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
        public async Task<IActionResult> ImportMovie(string imdbId)
        {
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

    }
}