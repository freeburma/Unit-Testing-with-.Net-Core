using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMovie;
using MvcMovie.Controllers;
using MvcMovie.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MVC_Movie_xUnit_Test
{
    /// <summary>
    /// Ref Medium Blog:        https://medium.com/wolox-driving-innovation/how-to-test-in-net-core-2-0-17008d5eb7bf
    /// Ref Single Memory Test: https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/in-memory
    /// Ref Controller Test:    https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing?view=aspnetcore-2.2
    /// </summary>
    public class SimpleUnitTest
    {

        private readonly MvcMovieContext _db;
        private readonly HttpClient _client;


        public SimpleUnitTest ()
        {
            var pathString = Path.GetFullPath(@"E:/Programing/ASP .Net/MvcMovie/MvcMovie");

            var configuration = new ConfigurationBuilder()
                                // Indicate the path for our source code
                                .SetBasePath(pathString)
                                 .Build();

            //// Create builder
            var builder = new WebHostBuilder()
                            // Set test environment
                            .UseEnvironment("Testing")
                            .UseStartup<Startup>()
                            .UseConfiguration(configuration);

            var server = new TestServer(builder);

            //// Create database context
            _db = server.Host.Services.GetService(typeof(MvcMovieContext)) as MvcMovieContext;

            ///// Create client to query server endpoints 
            _client = server.CreateClient();

            /// Creating movie list for Main db 
            foreach (var mov in MovieList)
            {
                _db.Add(mov);
                _db.SaveChanges();
            }



        }


        [Fact]
        public void Test1 ()
        {
            Assert.Equal(1, 1);
        }

        /// ======================== INTEGRATION TEST START ========================

        [Fact]
        ///
        /// Worked & Tested: 02-Aug-2019 05:13 pm
        /// Set the Movie List for Testing. 
        /// This will use the memory db rather than actual one because 
        /// you don't want to mess up your real database. Can't even use 
        /// the real database in the test method. 
        ///
        public void ShowReturnAllMovie ()
        {
            var movieList = MovieList.ToList();
            var movieCount = movieList.Count;

            Assert.Equal(5, movieCount);
        }

        [Fact]
        public void InsertMovie_in_Memory_Test ()
        {

            var options = new DbContextOptionsBuilder<MvcMovieContext>()
                .UseInMemoryDatabase(databaseName: $"InsertTest{DateTime.Now}")
                .Options;

            using (var context = new MvcMovieContext(options))
            {
                context.Movie.Add(new Movie { ID = 7, Title = "7", Price = 50M, Genre = "Romance", Rating = "G", ReleaseDate = DateTime.Now });
                context.Movie.Add(new Movie { ID = 8, Title = "8", Price = 60M, Genre = "Romance", Rating = "G", ReleaseDate = DateTime.Now });
                /// Adding to db 
                context.SaveChanges();
            };

            using (var context = new MvcMovieContext(options))
            {
                Assert.Equal(2, context.Movie.ToList().Count);
            }

        }


        [Fact]
        public void GetMovieByID_Test ()
        {

            var options = new DbContextOptionsBuilder<MvcMovieContext>()
                .UseInMemoryDatabase(databaseName: $"GetMovieByIDTestDb{DateTime.Now.ToString("dd/MM/yy hh:mm:ss")}")
                .Options;

            using (var context = new MvcMovieContext(options))
            {
                // var mov = new Movie { ID = id, Title = title, Price = price, Genre = genre, Rating = rating, ReleaseDate = releaseDate };
                // context.Movie.Add(mov);

                context.Movie.Add(new Movie { ID = 5, Title = "5", Price = 50M, Genre = "Romance", Rating = "G", ReleaseDate = DateTime.Now });
                context.Movie.Add(new Movie { ID = 6, Title = "6", Price = 60M, Genre = "Romance", Rating = "G", ReleaseDate = DateTime.Now });
                /// Adding to db 
                context.SaveChanges();
            };

            using (var context = new MvcMovieContext(options))
            {
                var movService = new MovieService(context);
                var mov = movService.GetMovieByID(6);


                Assert.Equal(6, mov.ID);
                Assert.Equal("6", mov.Title);
            }

        }

        [Fact]
        public async Task GetMovieByID_Test2 ()
        {


            // var mov = new Movie { ID = id, Title = title, Price = price, Genre = genre, Rating = rating, ReleaseDate = releaseDate };
            // context.Movie.Add(mov);

            _db.Movie.Add(new Movie { ID = 5, Title = "5", Price = 50M, Genre = "Romance", Rating = "G", ReleaseDate = DateTime.Now });
            _db.Movie.Add(new Movie { ID = 6, Title = "6", Price = 60M, Genre = "Romance", Rating = "G", ReleaseDate = DateTime.Now });
            /// Adding to db 
            _db.SaveChanges();

            var mov = _db.Movie
                      .Where(m => m.ID == 6)
                      .FirstOrDefault();

            /// Request the movie we've just added 
            var response = await _client.GetAsync($"/Movies/DetailsTest/{mov.ID}");

            /// Check the HTTP status OK = 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            /// Get JSOn of Movie from response 
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var movResponse = JsonConvert.DeserializeObject<Movie>(jsonResponse);

            Assert.Equal(mov.ID, movResponse.ID);
            Assert.Equal(mov.Title, movResponse.Title);


        }


        [Fact]
        public void FindMovies ()
        {
            var options = new DbContextOptionsBuilder<MvcMovieContext>()
                .UseInMemoryDatabase(databaseName: $"Find{DateTime.Now}")
                .Options;

            using (var context = new MvcMovieContext(options))
            {
                foreach (var mov in MovieList)
                {
                    context.Movie.Add(mov);
                    context.SaveChanges();
                }
            };

            using (var context = new MvcMovieContext(options))
            {
                var movService = new MovieService(context);
                var result = movService.FindByTitle("1");

                Assert.Equal(2, result.Count());
            }
        }
        /// ======================== INTEGRATION TEST   END ========================
        /// ########################################################################

        /// ======================== CONSTRUCTOR TEST START ========================

        [Fact]
        public void HomeController_Index_Test ()
        {
            /// Arrange 
            var homeController = new HomeController();

            /// Act 
            var result = homeController.Index();

            /// Assert
            var getActionName = Assert.IsType<ViewResult>(result);

            Assert.Equal("Index", getActionName.ViewName);
        }

        [Fact]
        public async Task MoviesController_Index_Test ()
        {
            /// Arrange 
            var controller = new MoviesController(_db);

            /// Act 
            var result = await controller.Index("", "1");

            /// Assert
            var viewResult = Assert.IsType<ViewResult>(result); 
            var model = Assert.IsType<MovieGenreViewModel>(viewResult.ViewData.Model);

            Assert.Equal("PG", model.movies.FirstOrDefault().Genre);
            Assert.Equal(2, model.movies.Count);
        }

        [Fact]
        public void MoviesController_BadRequest_Test()
        {
             /// Arrange 
            var controller = new MoviesController(_db);

            /// Act 
            var result = controller.BadRequestTest();

            /// Assert
            var viewResult = Assert.IsType<BadRequestResult>(result);
            Assert.IsType<BadRequestResult>(viewResult); 
        }

        [Fact]
        public async Task MoviesController_BadRequest_Edit_Post()
        {
             /// Arrange 
            var controller = new MoviesController(_db);

            var movie = new Movie()
            {
                ID = 12,
                Title = "", 
                ReleaseDate = DateTime.Now, 
                Genre = "", 
                Price = 0, 
                Rating = "G"
            };


            /// Act 
            var result = await controller.Edit(12, movie);

            /// Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(viewResult.Value); // Can be both True and False states


        }

        [Fact]
        public async Task MoviesController_SuccessfullyEdited_Edit_Post()
        {
             /// Arrange 
            var controller = new MoviesController(_db);

            var movie = new Movie()
            {
                ID = 1,
                Title = "Hello World", 
                ReleaseDate = DateTime.Now, 
                Genre = "PG", 
                Price = 100.00M, 
                Rating = "G"
            };


            /// Act 
            var result = await controller.Edit(1, movie);

            /// Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<Movie>(viewResult.ViewData.Model);

            Assert.Equal("Hello World", model.Title);
            
        }

        [Fact]
        public async Task MoviesController_Delete_Post ()
        {
             /// Arrange 
            var controller = new MoviesController(_db);

            /// Act 
            var result = await controller.DeleteConfirmed(1);

            /// Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", viewResult.ActionName);
        }

        [Fact]
        public void MoviesController_RedirectToAction_Test()
        {
             /// Arrange 
            var controller = new MoviesController(_db);

            /// Act 
            var result = controller.RedirectToActionTest();

            /// Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Null(viewResult.ControllerName);
            Assert.Equal("Index", viewResult.ActionName);
        }

        /// ======================== CONSTRUCTOR TEST  END  ========================

        /// <summary>
        /// Creating a Movie Db object data in Memory
        /// </summary>
       

        public List<Movie> MovieList => new List<Movie>
        {
            new Movie() {ID=1, Title = "1", Price=10.00M, Genre="PG", Rating = "G", ReleaseDate= DateTime.Now },
            new Movie() {ID=2, Title = "2", Price=20.00M, Genre="G", Rating = "G", ReleaseDate=DateTime.Now },
            new Movie() {ID=3, Title = "3", Price=30.00M, Genre="M", Rating = "G", ReleaseDate=DateTime.Now },
            new Movie() {ID=4, Title = "4", Price=40.00M, Genre="18+", Rating ="G", ReleaseDate=DateTime.Now },
            new Movie() {ID=11, Title = "11", Price=40.00M, Genre="18+",Rating ="G", ReleaseDate=DateTime.Now }
        };


        
    }
}
