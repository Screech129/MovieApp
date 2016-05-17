using Android.App;
using Android.OS;
using Android.Views;
using MovieApp.Fragments;
namespace MovieApp.Activities
{
    [Activity(Label = "DetailActivity")]
    public class DetailActivity : Activity
    {
        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.ActionBar);
            SetContentView(Resource.Layout.activity_detail);
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ActionBar.Title = "Movie Details";
            if (savedInstanceState == null)
            {
                FragmentTransaction fragTx = this.FragmentManager.BeginTransaction();

                fragTx.Add(Resource.Id.container, new DetailFragment())
                    .Commit();
            }
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Android.Resource.Id.Home)
            {
                Finish();
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}

