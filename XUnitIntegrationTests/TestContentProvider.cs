using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Util;
using Core;
using Model;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.XamarinAndroid;
using Xunit;

namespace XUnitIntegrationTests
{

    public class TestContentProvider
    {
        public const string LogTag = "TestDb";
        private static string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "movie.db3");

        private static SQLiteConnectionString connString = new SQLiteConnectionString(dbPath, false);
        private static SQLiteConnectionWithLock conn = new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), connString);
        private static SQLiteAsyncConnection db = new SQLiteAsyncConnection(() => conn);

        public TestContentProvider ()
        {


        }

        public async Task ArrangeEmptyDatabase ()
        {
            var provider = new MovieProvider(db);
            var helper = new MovieDbHelper(db);
            var movieTableExist = await helper.TableExists("Movies");
            var favoriteTableExist = await helper.TableExists("Favorites");
            if (movieTableExist)
            {
                provider.DropTables<Movies>();
            }
            if (favoriteTableExist)
            {
                provider.DropTables<Favorites>();
            }
        }

        public async Task ArrangeWithTables ()
        {
            await ArrangeEmptyDatabase();
            await CreateDatabase("Movies");
            await CreateDatabase("Favorites");
        }

        public async Task ArrangWithRecords ()
        {
            await ArrangeWithTables();
            var provider = new MovieProvider(db);
            var uri = Movies.ContentUri;
            await provider.Insert(uri, MockMovies());

            uri = Favorites.ContentUri;
            await provider.Insert(uri, MockFavorites());

        }


        private List<Movies> MockMovies ()
        {
            var movieEntries = new List<Movies>();
            var movie1 = new Movies()
            {
                MovieId = 1,
                MovieTitle = "Beauty and the Beast",
                Plot = "Girl and large dog fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTime.Now,
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            var movie2 = new Movies()
            {
                MovieId = 2,
                MovieTitle = "Happy Feet",
                Plot = "blahhhh",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTime.Now.AddDays(-34),
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            var movie3 = new Movies()
            {
                MovieId = 3,
                MovieTitle = "Beauty Beast",
                Plot = "fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTime.Now.AddDays(4),
                Reviews = "'s sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            var movie4 = new Movies()
            {
                MovieId = 4,
                MovieTitle = "Beauty",
                Plot = "Girl and large dog fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTime.Now.AddDays(-343),
                Reviews = "It's sooo ",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            movieEntries.Add(movie1);
            movieEntries.Add(movie2);
            movieEntries.Add(movie3);
            movieEntries.Add(movie4);
            return movieEntries;
        }

        private List<Favorites> MockFavorites ()
        {
            var favEntries = new List<Favorites>();
            var movie1 = new Favorites()
            {
                MovieId = 1,
                MovieTitle = "Aladdin",
                Plot = "Homeless man cons princess",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTime.Now.AddDays(-344),
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 10.5M
            };
            favEntries.Add(movie1);
            return favEntries;
        }

        public async Task<CreateTablesResult> CreateDatabase (string dbName)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(500);
            Log.Debug("IfStatement", "This was hit before If");
            try
            {
                if (dbName == "Movies")
                {
                    return await db.CreateTableAsync<Movies>(cts.Token);
                }
                else
                {
                    return await db.CreateTableAsync<Favorites>(cts.Token);

                }
            }
            catch (Exception ex)
            {
                Log.Debug("IfStatement", ex.ToString());
                throw;
            }


        }

        [Fact]
        public async Task Insert_MockMovie_ReturnAValidId ()
        {
            await ArrangeEmptyDatabase();
            try
            {
                await CreateDatabase("Movies").ContinueWith(async (Task<CreateTablesResult> t) =>
                {
                    var provider = new MovieProvider(db);
                    var uri = Movies.ContentUri;
                    var ids = await provider.Insert(uri, MockMovies());
                    Assert.True(ids.Count > 0);
                });
            }
            catch (Exception ex)
            {
                Log.Debug("FavoritesInsert", ex.ToString());
                throw;
            }
        }


        [Fact]
        public async Task Query_MovieDataBase_ReturnsAtLeastOneRecord ()
        {
            await ArrangWithRecords();

            var provider = new MovieProvider(db);
            var uri = Movies.ContentUri;

            var results = await provider.Query<Movies>(uri);
            Assert.True(results.Count > 0);
        }

        [Fact]
        public async Task Insert_MockFavorites_ReturnAValidId ()
        {
            await ArrangeEmptyDatabase();
            try
            {
                await CreateDatabase("Favorites").ContinueWith(async t =>
                {
                    var provider = new MovieProvider(db);
                    var uri = Favorites.ContentUri;
                    var ids = await provider.Insert(uri, MockFavorites());
                    Assert.True(ids.Count > 0);
                });

            }
            catch (Exception ex)
            {
                Log.Debug("FavoritesInsert", ex.ToString());
                throw;
            }

        }

        [Fact]
        public async Task BulkInsert_MockMovies_ReturnCountGreaterThan0 ()
        {
            await ArrangeEmptyDatabase();
            await CreateDatabase("Movies").ContinueWith(async t =>
             {
                 var provider = new MovieProvider(db);
                 var uri = Movies.ContentUri;
                 var returnNum = await provider.Insert(uri, MockMovies());
                 Assert.True(returnNum.Count == MockMovies().Count);
             });
        }

        [Fact]
        public async Task Query_FavoriteDataBase_ReturnsAtLeastOneRecord ()
        {
            await ArrangWithRecords();

            var provider = new MovieProvider(db);
            var uri = Favorites.ContentUri;

            var results = await provider.Query<Favorites>(uri);

            Assert.True(results.Count > 0);
        }

        [Fact]
        public async Task Query_MovieDataBaseSpecificId_ReturnsOneRecord ()
        {
            await ArrangWithRecords();

            var provider = new MovieProvider(db);
            var uri = Movies.BuildUri(2);

            var results = await provider.Query<Movies>(uri);
            Assert.True(results.Any(m => m.MovieId == 2));

        }

        [Fact]
        public void FailsOnPurpose ()
        {
            throw new Exception("Should not pass");
        }

        [Fact]
        public async Task FailsInTask ()
        {
            await Task.Run(() => { throw new Exception("AHHHHHHHHHHHHHHHHHHHHHHHH!!!!!"); }) ;
        }
    }
}