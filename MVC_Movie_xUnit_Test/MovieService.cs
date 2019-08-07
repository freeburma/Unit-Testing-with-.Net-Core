using MvcMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVC_Movie_xUnit_Test
{
    public class MovieService
    {
        private MvcMovieContext _db;

        public MovieService (MvcMovieContext db)
        {
            _db = db; 
        }

        public void Add(Movie model)
        {
            _db.Movie.Add(model);
            _db.SaveChanges(); 
            
        }

        public Movie GetMovieByID (int id)
        {
            return _db.Movie
                .Where(m => m.ID == id)
                .FirstOrDefault();
        }

        public IEnumerable<Movie> FindByTitle (string title)
        {
            return _db.Movie
                .Where(m => m.Title.Contains(title))
                .OrderBy(m => m.Title)
                .ToList();
        }

    }
}
