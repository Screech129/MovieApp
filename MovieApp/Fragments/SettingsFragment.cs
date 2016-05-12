
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
using Android.Preferences;
using MovieApp.Data;
using System.IO;

namespace MovieApp.Fragments
{
    public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public SettingsFragment ()
        {

        }
        public override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.pref_general);
            // Create your fragment here
        }

        public override void OnResume ()
        {
            base.OnResume();
            PreferenceManager.GetDefaultSharedPreferences(Activity).RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause ()
        {
            base.OnPause();
            PreferenceManager.GetDefaultSharedPreferences(Activity).RegisterOnSharedPreferenceChangeListener(this);

        }


        public async void OnSharedPreferenceChanged (ISharedPreferences sharedPreferences, string key)
        {
            Preference pref = FindPreference(key);
            if (pref.GetType() == typeof(ListPreference))
            {
                var listPref = (ListPreference)pref;
                pref.Summary = listPref.Entry;
            }
            else
            {
                pref.Summary = sharedPreferences.GetString(key, "");
            }
            var prefEditor = sharedPreferences.Edit();
            prefEditor.PutString(key, sharedPreferences.GetString(key, ""));
            prefEditor.Commit();
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");

            var movieCount = 0;//await db.Table<MovieContract.Movie>().CountAsync();
            if (movieCount > 0)
            {
                var provider = new MovieProvider();
                Android.Net.Uri uri = Movie.ContentUri;
                provider.DeleteRecords(uri, null, null);
            }
        }
    }
}

