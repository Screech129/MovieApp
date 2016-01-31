
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

namespace MovieApp
{
    public class PosterFragment : Fragment
    {
        List<string> paths = new List<string>();
        List<int> movieIds = new List<int>();
        public PosterFragment()
        {
            
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetHasOptionsMenu(true);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            var view = inflater.Inflate(Resource.Layout.fragment_main,container,false);
            var gridView = view.FindViewById<GridView>(Resource.Id.poster_grid);
            gridView.ItemClick += (sender, e) => {
                var movieId = e.Id;
                var detailsIntent = new Intent (this.Activity,typeof(DetailActivity));
                detailsIntent.PutExtra(Intent.ExtraText,movieId);
                StartActivity(detailsIntent);
            };
            GetMovies(gridView);
            return view;
        }

        public override void OnStart()
        {
            base.OnStart();

            GetMovies(this.View.FindViewById<GridView>(Resource.Id.poster_grid));
        }
        public async Task GetMovies(GridView view){
            try
            {
                
                var httpClient = new HttpClient();
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
                var sortBy = prefs.GetString(Resources.GetString(Resource.String.pref_sort_key),Resources.GetString(Resource.String.pref_sort_default));
                Task<string> getPopularMoviesJSON = httpClient.GetStringAsync("http://api.themoviedb.org/3/discover/movie?sort_by="+sortBy+"&api_key={Your API Key}");
                string popularMovies = await getPopularMoviesJSON;
                getMoviePosterPaths(popularMovies, view);

            }
            catch (Exception ex)
            {
                Log.Error("Image Adapter",ex.Message);
            }
        }

        private void getMoviePosterPaths (string popularMovies, GridView view){
            var pathsJson = new  JSONObject(popularMovies);
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

        }
    }
}

