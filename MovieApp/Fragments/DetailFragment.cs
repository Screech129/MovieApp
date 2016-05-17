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
using MovieApp.Data;
using Square.Picasso;
using System.Threading.Tasks;
using Uri = Android.Net.Uri;

namespace MovieApp.Fragments
{
    public class DetailFragment : Fragment
    {
        public DetailFragment ()
        {
            SetHasOptionsMenu(true);
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Intent intent = Activity.Intent;
            View rootView = inflater.Inflate(Resource.Layout.fragment_detail, container, false);
            if (intent != null && intent.HasExtra(Intent.ExtraText))
            {
                var movieId = intent.GetLongExtra(Intent.ExtraText, 0);
                var movieInfo = GetMoveInfo(movieId).Result.FirstOrDefault();

                BindFields(rootView, movieInfo);
            }
            return rootView;
        }

        private void BindFields (View rootView, MovieContract.MoviesTable movieInfo)
        {
            var titleTV = rootView.FindViewById<TextView>(Resource.Id.title_text);
            titleTV.Text = movieInfo.MovieTitle;
            var posterIV = rootView.FindViewById<ImageView>(Resource.Id.moviePoster);
            Picasso.With(this.Activity).Load("http://image.tmdb.org/t/p/w185/" + movieInfo.PosterPath).Into(posterIV);
            var plotTV = rootView.FindViewById<TextView>(Resource.Id.plot_text);
            plotTV.Text = movieInfo.Plot;
            var userRatingTV = rootView.FindViewById<TextView>(Resource.Id.rating_text);
            userRatingTV.Text = movieInfo.VoteAverage.ToString();
            var releaseDateTV = rootView.FindViewById<TextView>(Resource.Id.releaseDate_text);
            releaseDateTV.Text = movieInfo.ReleaseDate.ToString("MM/dd/yyyy");

            BindTrailers(rootView, movieInfo);
        }

        private void BindTrailers (View rootView, MovieContract.MoviesTable movieInfo)
        {
            var movieTrailerLL = rootView.FindViewById<LinearLayout>(Resource.Id.trailer_layout);
            if (!string.IsNullOrEmpty(movieInfo.Trailers))
            {
                var trailerList = movieInfo.Trailers.Split(',').ToList();
                trailerList.Remove("");
                var trailerNameList = new List<string>();
                foreach (var trailer in trailerList)
                {
                    var trailerSplit = trailer.Split(':');
                    trailerNameList.Add(trailerSplit[0]);
                }

                var trailerAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleExpandableListItem1, Android.Resource.Id.Text1, trailerNameList);
                for (int i = 0; i < trailerAdapter.Count; i++)
                {
                    View view = trailerAdapter.GetView(i, null, movieTrailerLL);
                    view.SetPadding(0, 10, 0, 5);
                    
                    view.Tag = trailerList[i].Split(':')[1];
                    view.Clickable = true;
                    view.Click += (sender, eventArgs) =>
                    {
                        var clickedView = (TextView)sender;
                        Log.Debug("ClickedView", clickedView.Tag.ToString());
                        OpenTrailer(clickedView.Tag.ToString());
                    };
                    View dividerView = new View(Activity);
                    dividerView.SetMinimumHeight(1);
                    dividerView.SetBackgroundColor(Android.Graphics.Color.DarkGray);

                    movieTrailerLL.AddView(view);
                    movieTrailerLL.AddView(dividerView);
                }
            }
        }

        private async Task<List<MovieContract.MoviesTable>> GetMoveInfo (long movieId)
        {
            var provider = new MovieProvider();
            var uri = MovieContract.MoviesTable.BuildUri(movieId);
            var movieList = await provider.QueryMovies(uri, null, movieId.ToString(), null);
            return movieList;
        }

        private void OpenTrailer (string youTubeUrl)
        {
            var youTubeLinkArray = youTubeUrl.Split(new string[] { ".com/" }, StringSplitOptions.None);

            try
            {
                var youTubeAppIntent = new Intent(Intent.ActionView, Uri.Parse("vnd.youtube:" + youTubeLinkArray[1]));
                StartActivity(youTubeAppIntent);
            }
            catch (Exception)
            {
                var uri = Uri.Parse(youTubeUrl);
                var youTubeIntent = new Intent(Intent.ActionView, uri);
                if (youTubeIntent.ResolveActivity(Activity.PackageManager) != null)
                {
                    StartActivity(youTubeIntent);
                }
                else
                {
                    Toast.MakeText(Activity, "Can't find YouTube app", ToastLength.Long).Show();
                    Log.Debug("Main Activity", "Couldn't call " + youTubeIntent + ", No YouTube App");
                }
            }
        }

    }
}