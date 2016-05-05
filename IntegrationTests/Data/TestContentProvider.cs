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
            context.DeleteDatabase("movie.db");
            
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

            return movieEntries;
        }

        private List<MovieContract.FavoritesTable> MockFavorites ()
        {
            var favEntries = new List<MovieContract.FavoritesTable>();
            var movie1 = new MovieContract.MoviesTable()
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

            return favEntries;
        }

        public void CreateDatabase ()
        {
            var db = new SQLiteConnection(dbPath);
            db.CreateTable<MovieContract.MoviesTable>();
            db.CreateTable<MovieContract.FavoritesTable>();

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
            provider.Insert(uri, MockMovies());
            var results = provider.Query(uri, null, null, null, "movie_id");
            Assert.IsNotNull(results);
           
        }

        [Test]
        public void Query_MovieDataBase_ReturnsAtLeastOneRecord ()
        {
            CreateDatabase();
            var provider = new MovieProvider();
            provider.OnCreate();
            Uri uri = MovieContract.MoviesTable.ContentUri;

            var c = provider.Query(uri, null, null, null, "movie_id");
            Assert.IsNotNull(c);
        }
    }
}