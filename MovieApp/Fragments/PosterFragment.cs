
using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net.Http;
using Org.Json;
using Android.Preferences;
using MovieApp.Activities;
using System.IO;
using Core;
using Model;
using SQLite.Net.Async;
using SQLite.Net;
using SQLite.Net.Platform.XamarinAndroid;

namespace MovieApp.Fragments
{
    public class PosterFragment : Fragment
    {
        private List<string> paths = new List<string>();
        private List<int> movieIds = new List<int>();
        private static string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");
        private static SQLiteConnectionString connString = new SQLiteConnectionString(dbPath, false);
        private static SQLiteConnectionWithLock conn = new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), connString);
        private static SQLiteAsyncConnection db = new SQLiteAsyncConnection(() => conn);

        private MovieProvider provider = new MovieProvider(db);

        public PosterFragment ()
        {

        }

        public override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetHasOptionsMenu(true);
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_main, container, false);
            var gridView = view.FindViewById<GridView>(Resource.Id.poster_grid);
            gridView.ItemClick += (sender, e) =>
            {
                var movieId = e.Id;
                var detailsIntent = new Intent(this.Activity, typeof(DetailActivity));
                detailsIntent.PutExtra(Intent.ExtraText, movieId);
                StartActivity(detailsIntent);
            };
            return view;
        }

        private async Task GetCachedMovies (GridView gridView)
        {
            try
            {
                var uri = Movies.ContentUri;

                var movies = await provider.Query<Movies>(uri);

                foreach (var movie in movies)
                {
                    paths.Add(movie.PosterPath);
                    movieIds.Add(movie.MovieId);
                }

                var imgAdapter = new ImageAdapter(Activity, paths, movieIds);
                gridView.Adapter = imgAdapter;
            }
            catch (Exception ex)
            {
                Log.Error("Image Adapter", ex.Message);
            }
        }

        public async override void OnStart ()
        {
            base.OnStart();
            var gridView = this.View.FindViewById<GridView>(Resource.Id.poster_grid);
            gridView.Adapter = null;
            movieIds.Clear();
            paths.Clear();
            var connString = new SQLiteConnectionString(dbPath,false);
            var conn = new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), connString);
            var db = new SQLiteAsyncConnection(()=>conn);

            var helper = new MovieDbHelper(db);
            var tableExist = await helper.TableExists("Movies");
            var movieCount = 0;
            if (tableExist)
            {
                movieCount = await db.Table<Movies>().CountAsync();
            }
            if (movieCount > 0)
            {
                await GetCachedMovies(gridView);
            }
            else
            {
                await GetMovies(gridView);
            }


        }

        public async Task GetMovies (GridView view)
        {
            try
            {
                var httpClient = new HttpClient();
                var prefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
                var sortBy = prefs.GetString(Resources.GetString(Resource.String.pref_sort_key), Resources.GetString(Resource.String.pref_sort_default));
                var getPopularMoviesJSON = httpClient.GetStringAsync("http://api.themoviedb.org/3/discover/movie?sort_by=" + sortBy + "&api_key=51be394ff82a4dec506f5cf2ce21f6d4");
                var movieResult = await getPopularMoviesJSON;
                await GetMoviePosterPaths(movieResult, view);

            }
            catch (Exception ex)
            {
                Log.Error("Image Adapter", ex.Message);
            }
        }

        private async Task GetMoviePosterPaths (string movieResult, GridView view)
        {
            var pathsJson = new JSONObject(movieResult);
            var pathArray = pathsJson.GetJSONArray("results");
            paths.Clear();
            for (var i = 0; i < pathArray.Length(); i++)
            {
                var posterPath = pathArray.GetJSONObject(i);
                paths.Add(posterPath.GetString("poster_path"));
                movieIds.Add(posterPath.GetInt("id"));
            }
            var imgAdapter = new ImageAdapter(Activity, paths, movieIds);
            view.Adapter = imgAdapter;
            await GetMoveInfo(movieIds, paths);
        }

        private async Task GetMoveInfo (List<int> movieIds, List<string> paths)
        {
            var httpClient = new HttpClient();
            var movieList = new List<Movies>();
            var count = 0;
            foreach (var movie in movieIds)
            {
                var getJSON = httpClient.GetStringAsync("http://api.themoviedb.org/3/movie/" + movie + "?api_key=51be394ff82a4dec506f5cf2ce21f6d4&append_to_response=reviews,trailers");
                var movieInfoStringBuilder = new StringBuilder();
                movieInfoStringBuilder.Append(getJSON.Result);
                var JSONString = movieInfoStringBuilder.ToString();
                var trailerObjects = new List<object>();
                try
                {
                    var movieInfoJson = new JSONObject(JSONString);
                    var trailerInfo = new JSONObject(movieInfoJson.GetString("trailers"));
                    var youTubeInfo = new JSONArray(trailerInfo.GetString("youtube"));
 
                    for (var i=0;i<youTubeInfo.Length();i++)
                    {
                        var jsonInfo = new JSONObject(youTubeInfo.GetString(i));
                        var trailer = new {
                            MovieId = movieInfoJson.GetInt("id"),
                            Name = jsonInfo.GetString("name"),
                            Path = jsonInfo.GetString("source")
                        };

                        trailerObjects.Add(trailer);
                    }
                    var newMovie = new Movies()
                    {
                        MovieTitle = movieInfoJson.GetString("original_title"),
                        PosterPath = paths[count],
                        Plot = movieInfoJson.GetString("overview"),
                        VoteAverage = movieInfoJson.GetInt("vote_average"),
                        ReleaseDate = DateTime.Parse(movieInfoJson.GetString("release_date")),
                        MovieId = movieInfoJson.GetInt("id"),
                        Reviews = movieInfoJson.GetString("reviews"),
                    };
                    foreach (var trailer in trailerObjects)
                    {
                        var t = trailer.GetType();
                        newMovie.Trailers += t.GetProperty("Name").GetValue(trailer)+ ": www.youtube.com/" + t.GetProperty("Path").GetValue(trailer) +",";
                    }
                    
                    
                    movieList.Add(newMovie);
                    count++;
                }
                catch (Exception ex)
                {
                    Log.Error("DetailActivityJSON", ex.Message);
                }
            }
            await CacheMovies(movieList);

        }

        private async Task CacheMovies (List<Movies> movies)
        {
            var movieHelper = new MovieDbHelper(db);

            await movieHelper.CreateTable(typeof(Movies)).ContinueWith(async t =>
            {
                var uri = Movies.ContentUri;
                await provider.DeleteRecords(uri, null, null).ContinueWith(async t2 =>
                {
                    await provider.Insert(uri, movies);
                });
            });

        }
    }
}

