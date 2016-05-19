using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content;
using Core;

namespace MovieApp.Activities
{
    [Activity(Label = "MovieApp", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Core.Ioc.Container.Register<ILogger>(new AndroidLogger());
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

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


