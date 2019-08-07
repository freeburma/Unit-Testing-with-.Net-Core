using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MvcMovie.Models
{
    public class MovieGenreViewModel
    {
        public List<Movie> movies;              // A list of movies 
        public SelectList genres;               // A SelectList containing the list of genres. This will allow the user 
                                                // to select a genere from the list 
        public string movieGenre { get; set; } // Contains the selected genere 

    }// end class 
}
