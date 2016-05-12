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
using Android.Database.Sqlite;
using System.IO;
using System.Threading.Tasks;
using Android.Util;
using System.Threading;
using Xunit;
using Xunit.Sdk;
using Realms;
using MovieApp.Data;

namespace XunitIntegrationTest
{

    public class TestContentProvider
    {
        public const string LogTag = "TestDb";
        Context context = Application.Context;
        static RealmConfiguration config = new RealmConfiguration();
        Realm realm = Realm.GetInstance(config);

        public TestContentProvider ()
        {
            var config = new RealmConfiguration();
            Realm.DeleteRealm(config);
        }

        private List<MovieContract.Movie> MockMovies ()
        {
            var movieEntries = new List<MovieContract.Movie>();
            var movie1 = new MovieContract.Movie()
            {
                MovieId = 1,
                MovieTitle = "Beauty and the Beast",
                Plot = "Girl and large dog fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTimeOffset.Now,
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5F
            };
            var movie2 = new MovieContract.Movie()
            {
                MovieId = 2,
                MovieTitle = "Happy Feet",
                Plot = "blahhhh",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTimeOffset.Now.AddDays(5),
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5F
            };
            var movie3 = new MovieContract.Movie()
            {
                MovieId = 3,
                MovieTitle = "Beauty Beast",
                Plot = "fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTimeOffset.Now.AddYears(4),
                Reviews = "'s sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5F
            };
            var movie4 = new MovieContract.Movie()
            {
                MovieId = 4,
                MovieTitle = "Beauty",
                Plot = "Girl and large dog fall in love",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTimeOffset.Now.AddYears(-30),
                Reviews = "It's sooo ",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 8.5F
            };
            movieEntries.Add(movie1);
            movieEntries.Add(movie2);
            movieEntries.Add(movie3);
            movieEntries.Add(movie4);
            return movieEntries;
        }

        private List<MovieContract.Favorite> MockFavorites ()
        {
            var favEntries = new List<MovieContract.Favorite>();
            var movie1 = new MovieContract.Favorite()
            {
                MovieId = 1,
                MovieTitle = "Aladdin",
                Plot = "Homeless man cons princess",
                PosterPath = "http://www.google.com",
                ReleaseDate = DateTimeOffset.Now.AddMonths(-68),
                Reviews = "It's sooo splendubious",
                Trailers = "http://www.yahoo.com",
                VoteAverage = 10.5F
            };
            favEntries.Add(movie1);
            return favEntries;
        }

        //[Fact]
        public void Insert_MockMovie_ReturnAValidId ()
        {
            var id = 0;

            var cts = new CancellationTokenSource();
            try
            {
                var provider = new MovieProvider();
                Uri uri = MovieContract.Movie.ContentUri;
                var returnUriRaw = provider.Insert(uri, MockMovies());
                var returnUri = returnUriRaw;
                id = int.Parse(returnUri.PathSegments[3]);
                Assert.True(id > 0);
            }
            catch (Exception ex)
            {
                Log.Debug("FavoritesInsert", ex.ToString());
                throw;
            }
        }


        //[Fact]
        public void Query_MovieDataBase_ReturnsAtLeastOneRecord ()
        {
            Insert_MockMovie_ReturnAValidId();
            var provider = new MovieProvider();
            Uri uri = MovieContract.Movie.ContentUri;

            var results = provider.QueryMovies(uri, null, null, "movie_id");
            Assert.True(results.Count() > 0);
        }

        [Fact]
        public void Insert_MockFavorites_ReturnAValidId ()
        {
            var id = 0;

            var cts = new CancellationTokenSource();
            try
            {
                var provider = new MovieProvider();
                Uri uri = MovieContract.Favorite.ContentUri;
                var returnUriRaw = provider.Insert(uri, MockFavorites());
                var returnUri = returnUriRaw;
                id = int.Parse(returnUri.PathSegments[3]);
                Assert.True(id > 0);

            }
            catch (Exception ex)
            {
                Log.Debug("FavoritesInsert", ex.ToString());
                throw;
            }

        }

       // [Fact]
        public void BulkInsert_MockMovies_ReturnCountGreaterThan0 ()
        {
            var returnNum = 0;
            var provider = new MovieProvider();
            Uri uri = MovieContract.Movie.ContentUri;
            returnNum = provider.BulkInsert(uri, MockMovies());
            Assert.True(returnNum > 0);
        }

       // [Fact]
        public void Query_FavoriteDataBase_ReturnsAtLeastOneRecord ()
        {
            Insert_MockMovie_ReturnAValidId();
            var provider = new MovieProvider();
            Uri uri = MovieContract.Favorite.ContentUri;

            var results = provider.QueryFavorites(uri, null, null, "movie_id");

            Assert.True(results.Count() > 0);

        }
    }
}