
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net.Http;
using Org.Json;
using Android.Preferences;
using MovieApp.Activities;
using MovieApp.Data;
using Uri = Android.Net.Uri;
using System.IO;
using Android.Database.Sqlite;

namespace MovieApp.Fragments
{
    public class PosterFragment : Fragment
    {
        List<string> paths = new List<string>();
        List<int> movieIds = new List<int>();
        MovieProvider provider = new MovieProvider();
        static string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");

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

        private void GetCachedMovies (GridView gridView)
        {
            try
            {
                Uri uri = Movie.ContentUri;

                var movies = provider.QueryMovies(uri, null, null, null);

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

            var tableExist = true; //await MovieDbHelper.TableExists<Movie>(db, "Movies");
            var movieCount = 0; //await db.Table<Movie>().CountAsync();
            if (tableExist && movieCount > 0)
            {
                GetCachedMovies(gridView);
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
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
                var sortBy = prefs.GetString(Resources.GetString(Resource.String.pref_sort_key), Resources.GetString(Resource.String.pref_sort_default));
                Task<string> getPopularMoviesJSON = httpClient.GetStringAsync("http://api.themoviedb.org/3/discover/movie?sort_by=" + sortBy + "&api_key=51be394ff82a4dec506f5cf2ce21f6d4");
                string movieResult = await getPopularMoviesJSON;
                GetMoviePosterPaths(movieResult, view);

            }
            catch (Exception ex)
            {
                Log.Error("Image Adapter", ex.Message);
            }
        }

        private void GetMoviePosterPaths (string movieResult, GridView view)
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
            GetMoveInfo(movieIds, paths);
        }

        private void GetMoveInfo (List<int> movieIds, List<string> paths)
        {
            var httpClient = new HttpClient();
            List<Movie> movieList = new List<Movie>();
            foreach (var movie in movieIds)
            {
                Task<string> getJSON = httpClient.GetStringAsync("http://api.themoviedb.org/3/movie/" + movie + "?api_key=51be394ff82a4dec506f5cf2ce21f6d4&append_to_response=reviews,trailers");
                var movieInfoStringBuilder = new StringBuilder();
                movieInfoStringBuilder.Append(getJSON.Result);
                var JSONString = movieInfoStringBuilder.ToString();
                try
                {
                    JSONObject movieInfoJson = new JSONObject(JSONString);

                    var newMovie = new Movie()
                    {
                        MovieTitle = movieInfoJson.GetString("original_title"),
                        PosterPath = movieInfoJson.GetString("poster_path"),
                        Plot = movieInfoJson.GetString("overview"),
                        VoteAverage = movieInfoJson.GetInt("vote_average"),
                        ReleaseDate = DateTimeOffset.Parse(movieInfoJson.GetString("release_date")),
                        MovieId = movieInfoJson.GetInt("id"),
                        Reviews = movieInfoJson.GetString("reviews"),
                        Trailers = movieInfoJson.GetString("trailers")
                    };
                    movieList.Add(newMovie);

                }
                catch (Exception ex)
                {
                    Log.Error("DetailActivityJSON", ex.Message);
                }
            }
            CacheMovies(movieList);

        }

        private void CacheMovies (List<Movie> movies)
        {
            Uri uri = Movie.ContentUri;
            provider.DeleteRecords(uri, null, null);
            provider.BulkInsert(uri, movies);

        }
    }
}

