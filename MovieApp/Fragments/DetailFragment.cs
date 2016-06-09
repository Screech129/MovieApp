using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using System.Threading.Tasks;
using Uri = Android.Net.Uri;
using Model;
using Core;
using SQLite.Net;
using SQLite.Net.Platform.XamarinAndroid;
using SQLite.Net.Async;
using System.IO;

namespace MovieApp.Fragments
{

    public class DetailFragment : Fragment
    {
        public const string DETAIL_URI = "URI";
        private Uri globalUri;
        private static string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");
        private static SQLiteConnectionString connString = new SQLiteConnectionString(dbPath, false);
        private static SQLiteConnectionWithLock conn = new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), connString);
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(() => conn);
        public DetailFragment ()
        {
            SetHasOptionsMenu(true);
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var arguments = Arguments;
            var intent = Activity.Intent;
            var rootView = inflater.Inflate(Resource.Layout.fragment_detail, container, false);
            if (arguments == null && intent.Data == null) return rootView;
            globalUri = arguments == null ? intent.Data : (Uri)arguments.GetParcelable(DETAIL_URI);

            var movieId = globalUri.PathSegments.Last();
            var movieInfo = GetMoveInfo(long.Parse(movieId)).Result.FirstOrDefault();
            BindFields(rootView, movieInfo);
            return rootView;
        }

        private void BindFields (View rootView, Movies movieInfo)
        {
            var titleTv = rootView.FindViewById<TextView>(Resource.Id.title_text);
            titleTv.Text = movieInfo.MovieTitle;
            var posterIv = rootView.FindViewById<ImageView>(Resource.Id.moviePoster);
            Picasso.With(this.Activity).Load("http://image.tmdb.org/t/p/w185/" + movieInfo.PosterPath).Into(posterIv);
            var plotTv = rootView.FindViewById<TextView>(Resource.Id.plot_text);
            plotTv.Text = movieInfo.Plot;
            var userRatingTv = rootView.FindViewById<TextView>(Resource.Id.rating_text);
            userRatingTv.Text = movieInfo.VoteAverage.ToString();
            var releaseDateTv = rootView.FindViewById<TextView>(Resource.Id.releaseDate_text);
            releaseDateTv.Text = movieInfo.ReleaseDate.ToString("MM/dd/yyyy");
            var reviewTv = rootView.FindViewById<TextView>(Resource.Id.review_text);
            reviewTv.Text = movieInfo.Reviews;

            var starBtn = rootView.FindViewById<CheckBox>(Resource.Id.favorite);
            starBtn.Checked = movieInfo.IsFavorite;

            starBtn.Click += async (sender, e) =>
            {
                
                var movieId = long.Parse(globalUri.PathSegments.Last());
                var provider = new MovieProvider(db);
                var movieUri = Movies.BuildUri(movieId);
                var faveUri = Favorites.BuildUri(movieId);
                var currentMovieList = await GetMoveInfo(movieId);
                var currentMovie = currentMovieList.FirstOrDefault();
                if (currentMovie != null) currentMovie.IsFavorite = starBtn.Checked;
                provider.Update(movieUri, currentMovieList, null, null);
                if (starBtn.Checked)
                {
                    var favoriteList = new List<Favorites>();
                    var favorite = new Favorites()
                    {
                        MovieId = currentMovie.MovieId,
                        MovieTitle = currentMovie.MovieTitle,
                        Plot = currentMovie.Plot,
                        PosterPath = currentMovie.PosterPath,
                        ReleaseDate = currentMovie.ReleaseDate,
                        Reviews = currentMovie.Reviews,
                        Trailers = currentMovie.Trailers,
                        VoteAverage = currentMovie.VoteAverage
                    };
                    var dbHelper = new MovieDbHelper(db);
                    if (!await dbHelper.TableExists("Favorites"))
                    {
                        await db.CreateTableAsync<Favorites>();
                    }
                    favoriteList.Add(favorite);
                    await provider.Insert(faveUri, favoriteList);
                }
                else
                {
                    await provider.DeleteRecords(faveUri, faveUri.Parse().Id.ToString(), null);
                }
            };

            BindTrailers(rootView, movieInfo);
        }

        private void BindTrailers (View rootView, Movies movieInfo)
        {
            var movieTrailerLl = rootView.FindViewById<LinearLayout>(Resource.Id.trailer_layout);
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
                for (var i = 0; i < trailerAdapter.Count; i++)
                {
                    var view = trailerAdapter.GetView(i, null, movieTrailerLl);
                    view.SetPadding(0, 10, 0, 5);

                    view.Tag = trailerList[i].Split(':')[1];
                    view.Clickable = true;
                    view.Click += (sender, eventArgs) =>
                    {
                        var clickedView = (TextView)sender;
                        Log.Debug("ClickedView", clickedView.Tag.ToString());
                        OpenTrailer(clickedView.Tag.ToString());
                    };
                    var dividerView = new View(Activity);
                    dividerView.SetMinimumHeight(1);
                    dividerView.SetBackgroundColor(Android.Graphics.Color.DarkGray);

                    movieTrailerLl.AddView(view);
                    movieTrailerLl.AddView(dividerView);
                }
            }
        }

        private async Task<List<Movies>> GetMoveInfo (long movieId)
        {
            var provider = new MovieProvider(db);
            var uri = Movies.BuildUri(movieId);
            var movieList = await provider.Query<Movies>(uri);
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