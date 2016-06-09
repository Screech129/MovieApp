using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content;
using Core;
using MovieApp.Fragments;

namespace MovieApp.Activities
{
    [Activity(Label = "MovieApp", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity,PosterFragment.ICallback
    {
        bool twoPane;
        private const string DETAILFRAGMENT_TAG = "DFTAG";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Core.Ioc.Container.Register<ILogger>(new AndroidLogger());
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            if (FindViewById(Resource.Id.movie_detail_container) != null)
            {
                twoPane = true;
                if (savedInstanceState == null)
                {
                    FragmentManager.BeginTransaction()
                        .Replace(Resource.Id.movie_detail_container,
                        new DetailFragment(), DETAILFRAGMENT_TAG)
                        .Commit();

                }
            }
            else
            {
                twoPane = false;
            }
        }

        public void OnItemSelected (Android.Net.Uri movieUri)
        {

            if (twoPane)
            {
                Bundle args = new Bundle();
                args.PutParcelable(DetailFragment.DETAIL_URI, movieUri);

                DetailFragment fragment = new DetailFragment {Arguments = args};

                FragmentManager.BeginTransaction()
                         .Replace(Resource.Id.movie_detail_container, fragment, DETAILFRAGMENT_TAG)
                         .Commit();
            }
            else
            {
                var intent = new Intent(this, typeof(DetailActivity))
                             .SetData(movieUri);
                StartActivity(intent);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnMenuItemSelected(int featureId, IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                StartActivity(new Intent(this, typeof(SettingsActivity)));
                return true;
            }
            return base.OnMenuItemSelected(featureId, item);
        }
    }
}


