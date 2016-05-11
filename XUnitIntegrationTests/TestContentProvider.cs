using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MovieApp;
using Uri = Android.Net.Uri;
using Android.Database;
using MovieApp.Data;
using Android.Database.Sqlite;
using SQLite;
using System.IO;
using System.Threading.Tasks;
using Android.Util;
using System.Threading;
using Xunit;
using Xunit.Sdk;

namespace XunitIntegrationTest
{

    public class TestContentProvider
    {
        public const string LogTag = "TestDb";
        Context context = Application.Context;
        string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");

        public TestContentProvider ()
        {
            var dbHelper = new MovieDbHelper(context);
            dbHelper.DeleteDatabase();

        }

        private List<MovieContract.MoviesTable> MockMovies ()
        {
            var movieEntries = new List<MovieContract.MoviesTable>();
            var movie1 = new MovieContract.MoviesTable()
            {
                MovieId = 1,
                MovieTitle = "Beauty and the Beast",
                Plot = "Girl and large dog fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = 1419033600,
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            var movie2 = new MovieContract.MoviesTable()
            {
                MovieId = 2,
                MovieTitle = "Happy Feet",
                Plot = "blahhhh",
                PosterPath = "http://www.google.com",
                ReleaseDate = 1419033600,
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            var movie3 = new MovieContract.MoviesTable()
            {
                MovieId = 3,
                MovieTitle = "Beauty Beast",
                Plot = "fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = 1419033600,
                Reviews = "'s sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5M
            };
            var movie4 = new MovieContract.MoviesTable()
            {
                MovieId = 4,
                MovieTitle = "Beauty",
                Plot = "Girl and large dog fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = 1419033600,
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

        private List<MovieContract.FavoritesTable> MockFavorites ()
        {
            var favEntries = new List<MovieContract.FavoritesTable>();
            var movie1 = new MovieContract.FavoritesTable()
            {
                MovieId = 1,
                MovieTitle = "Aladdin",
                Plot = "Homeless man cons princess",
                PosterPath = "http://www.google.com",
                ReleaseDate = 1419033600,
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
            var db = new SQLiteAsyncConnection(dbPath);
            Log.Debug("IfStatement", "This was hit before If");
            try
            {
                if (dbName == "Movies")
                {
                    return await db.CreateTableAsync<MovieContract.MoviesTable>();
                }
                else
                {
                    return await db.CreateTableAsync<MovieContract.FavoritesTable>();

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
            var id = 0;

            var cts = new CancellationTokenSource();
            try
            {
                await CreateDatabase("Movies").ContinueWith(async t =>
                {
                    var provider = new MovieProvider();
                    Uri uri = MovieContract.MoviesTable.ContentUri;
                    var returnUriRaw = await provider.Insert(uri, MockMovies());
                    var returnUri = returnUriRaw;
                    id = int.Parse(returnUri.PathSegments[3]);
                    Assert.True(id > 0);
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
            await Insert_MockMovie_ReturnAValidId().ContinueWith(async t =>
            {
                var provider = new MovieProvider();
                Uri uri = MovieContract.MoviesTable.ContentUri;

                var results = await provider.QueryMovies(uri, null, null, "movie_id");
                Assert.True(results.Count > 0);
            });

        }

        [Fact]
        public async Task Insert_MockFavorites_ReturnAValidId ()
        {
            var id = 0;

            var cts = new CancellationTokenSource();
            try
            {
                await CreateDatabase("Favorites").ContinueWith(async t =>
                {
                    var provider = new MovieProvider();
                    Uri uri = MovieContract.FavoritesTable.ContentUri;
                    var returnUriRaw = await provider.Insert(uri, MockFavorites());
                    var returnUri = returnUriRaw;
                    id = int.Parse(returnUri.PathSegments[3]);
                    Assert.True(id > 0);
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
            var returnNum = 0;
            await CreateDatabase("Movies").ContinueWith(async t =>
             {
                 var provider = new MovieProvider();
                 Uri uri = MovieContract.MoviesTable.ContentUri;
                 returnNum = await provider.BulkInsert(uri, MockMovies());
                 Assert.True(returnNum > 0);
             });
        }

        [Fact]
        public async Task Query_FavoriteDataBase_ReturnsAtLeastOneRecord ()
        {
            await Insert_MockMovie_ReturnAValidId().ContinueWith(async t =>
            {
                var provider = new MovieProvider();
                Uri uri = MovieContract.FavoritesTable.ContentUri;

                var results = await provider.QueryFavorites(uri, null, null, "movie_id");

                Assert.True(results.Count > 0);
            });

        }
    }
}