using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MvcMovie.Models;

namespace MvcMovie.Controllers
{

    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;


        public MoviesController (MvcMovieContext context)
        {
            _context = context;
        }


        // HttpPost method added 
        //[HttpPost]
        //public string Index(string searchString, bool notUsed)
        //{
        //    return "From [HttpPost] index: Filter on " + searchString + " has not implemented yet.";
        //}

        /*
         public async Task<IActionResult> Index(string searchString) // Comment for adding genere 
         {
            // The query is only defined at this point
            var movies = from m in _context.Movie
                         select m; 

            if (!String.IsNullOrEmpty(searchString))
            {
                // To test in URL: http://localhost:58606/Movies?searchString=Ghost
                movies = movies.Where(s => s.Title.Contains(searchString)); 

                // After renamed "searchString" with "id": To test url: http://localhost:58606/Movies/Index/ghost
                // movies = movies.Where(s => s.Title.Contains(id));
            }

            return View(await movies.ToListAsync());
         }
         */

        // GET: Movies
        public async Task<IActionResult> Index (string movieGenre, string searchString)
        {
            // Use LINQ to get list of genres from database 
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            // The query is only defined at this point
            var movies = from m in _context.Movie
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                // To test in URL: http://localhost:58606/Movies?searchString=Ghost
                movies = movies.Where(s => s.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre.Contains(movieGenre));
            }

            var movieGenreVM = new MovieGenreViewModel();
            movieGenreVM.genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            movieGenreVM.movies = await movies.ToListAsync();


            return View(movieGenreVM);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        public async Task<IActionResult> DetailsTest (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return Json(movie);
        }


        // GET: Movies/Create
        public IActionResult Create ()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ([Bind("ID,Title,ReleaseDate,Genre,Price, Rating")] Movie movie)
        {
            // Checking fo rthe validating
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }


        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit (int? id) // Models/Movies/Edit.cshtml
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // Bind attribute: To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit (int id, [Bind("ID,Title,ReleaseDate,Genre,Price, Rating")] Movie movie)
        public async Task<IActionResult> Edit (int id, [FromBody] Movie movie)
        {

            if (ModelState.IsValid)
            {
                if (id != movie.ID)
                {
                    return NotFound();
                }

                var movToEdit = _context.Movie.Find(id);
                 
                if (movToEdit == null)
                    return BadRequest(ModelState);

                try
                {
                    movToEdit.ID = movie.ID; 
                    movToEdit.Title = movie.Title; 
                    movToEdit.Price = movie.Price; 
                    movToEdit.ReleaseDate = movie.ReleaseDate;
                    movToEdit.Genre = movie.Genre; 
                    movToEdit.Rating = movie.Rating; 

                    _context.Update(movToEdit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // return RedirectToAction(nameof(Index));
            }
            else
            {
                return BadRequest(ModelState);
            }

            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // A lambda expression is passed in to SingleOrDefaultAsync to select movie entities that match the 
            // route data or query string value.
            var movie = await _context.Movie
                .SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id)
        {
            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists (int id)
        {
            return _context.Movie.Any(e => e.ID == id);
        }

        /// =======================================================================
        /// Testing methods
        /// =======================================================================

        public IActionResult BadRequestTest ()
        {
            return BadRequest();
        }

        public IActionResult RedirectToActionTest ()
        {
            return RedirectToAction(nameof(Index));
        }


    }
}
