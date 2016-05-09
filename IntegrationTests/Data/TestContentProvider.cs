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
using NUnit.Framework;
using MovieApp;
using Uri = Android.Net.Uri;
using Android.Database;
using MovieApp.Data;
using Android.Database.Sqlite;
using SQLite;
using System.IO;

namespace IntegrationTests.Data
{
    [TestFixture]
    public class TestContentProvider
    {
        public const string LogTag = "TestDb";
        Context context = Application.Context;
        string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");
        [SetUp]
        public void Setup ()
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

        public void CreateDatabase ()
        {
            var db = new SQLiteAsyncConnection(dbPath);
            db.CreateTableAsync<MovieContract.MoviesTable>();
            db.CreateTableAsync<MovieContract.FavoritesTable>();

            Assert.IsNotNull(db.Table<MovieContract.MoviesTable>());
            Assert.IsNotNull(db.Table<MovieContract.FavoritesTable>());
        }

        [TearDown]
        public void Tear () { }

       [Test]
        public void Insert_MockMovie_ReturnAValidId ()
        {
            CreateDatabase();
            var provider = new MovieProvider();
            Uri uri = MovieContract.MoviesTable.ContentUri;
            var returnUri = provider.Insert(uri, MockMovies());
            var id = int.Parse(returnUri.PathSegments[3]);
            Assert.IsTrue(id > 0);
           
        }

       [Test]
        public void Query_MovieDataBase_ReturnsAtLeastOneRecord ()
        {
            Insert_MockMovie_ReturnAValidId();
            var provider = new MovieProvider();
            Uri uri = MovieContract.MoviesTable.ContentUri;

            var results = provider.QueryMovies(uri, null, null, "movie_id");
            Assert.IsNotNull(results);
        }

      [Test]
        public void Insert_MockFavorites_ReturnAValidId ()
        {
            CreateDatabase();
            var provider = new MovieProvider();
            Uri uri = MovieContract.FavoritesTable.ContentUri;
            var returnUri = provider.Insert(uri, MockFavorites());
            var id = int.Parse(returnUri.PathSegments[3]);
            Assert.IsTrue(id > 0);

        }

        [Test]
        public void BulkInsert_MockMovies_ReturnAValidId ()
        {
            CreateDatabase();
            var provider = new MovieProvider();
            Uri uri = MovieContract.MoviesTable.ContentUri;
            var returnNum = provider.BulkInsert(uri, MockMovies());
            var id = returnNum;
            Assert.IsTrue(id > 0);

        }

       [Test]
        public void Query_FavoriteDataBase_ReturnsAtLeastOneRecord ()
        {
            Insert_MockMovie_ReturnAValidId();
            var provider = new MovieProvider();
            Uri uri = MovieContract.FavoritesTable.ContentUri;

            var results = provider.QueryFavorites(uri, null, null, "movie_id");
            Assert.IsNotNull(results);
        }
    }
}