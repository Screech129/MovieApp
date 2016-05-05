 
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
using System.Net.Http;
using System.Threading.Tasks;
using Org.Json;
using Android.Util;
using Square.Picasso;

namespace MovieApp.Activities
{
    [Activity(Label = "DetailActivity")]			
    public class DetailActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.ActionBar);
            SetContentView(Resource.Layout.activity_detail);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                FragmentTransaction fragTx = this.FragmentManager.BeginTransaction();

                fragTx.Add(Resource.Id.container, new PlaceHolderFragment())
                    .Commit();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if(id == Android.Resource.Id.Home){
                Finish();
            }

            return base.OnOptionsItemSelected(item);
        }
    }



    public class PlaceHolderFragment:Fragment
    {
        public PlaceHolderFragment()
        {
            SetHasOptionsMenu(true);
        }

     public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Intent intent = Activity.Intent;
            View rootView = inflater.Inflate(Resource.Layout.fragment_detail, container, false);
            if (intent != null && intent.HasExtra (Intent.ExtraText))
            {
                var movieId = intent.GetLongExtra(Intent.ExtraText, 0);
                var movieRequest = getMoveInfo(movieId);
                var movieInfo = movieRequest;

                var titleTV = rootView.FindViewById<TextView>(Resource.Id.title_text);
                titleTV.Text = movieRequest[0];
                var posterIV = rootView.FindViewById<ImageView>(Resource.Id.moviePoster);
                Picasso.With(this.Activity).Load("http://image.tmdb.org/t/p/w185/"+movieRequest[1]).Into(posterIV);             
                var plotTV = rootView.FindViewById<TextView>(Resource.Id.plot_text);
                plotTV.Text = movieRequest[2]; 
                var userRatingTV = rootView.FindViewById<TextView>(Resource.Id.rating_text);
                userRatingTV.Text = movieRequest[3];
                var releaseDateTV = rootView.FindViewById<TextView>(Resource.Id.releaseDate_text);
                releaseDateTV.Text = movieRequest[4];
            }
            return rootView;
        }

        private List<string> getMoveInfo(long movieId)
        {
            var httpClient = new HttpClient();

            List<string> jsonValue = new List<string>();
            Task<string> getJSON = httpClient.GetStringAsync("http://api.themoviedb.org/3/movie/"+movieId+ "?api_key=51be394ff82a4dec506f5cf2ce21f6d4");
            var movieInfoStringBuilder = new StringBuilder();
            movieInfoStringBuilder.Append( getJSON.Result);
            var JSONString = movieInfoStringBuilder.ToString();
            try
            {
                JSONObject movieInfoJson = new JSONObject (JSONString);
                jsonValue.Add( movieInfoJson.GetString("original_title"));
                jsonValue.Add( movieInfoJson.GetString("poster_path"));
                jsonValue.Add(movieInfoJson.GetString("overview"));
                jsonValue.Add(movieInfoJson.GetString("vote_average"));
                jsonValue.Add(movieInfoJson.GetString("release_date"));
            }
            catch (Exception ex)
            {
                Log.Error("DetailActivityJSON",ex.Message); 
            }
            return jsonValue;

           
        }
    }
}

