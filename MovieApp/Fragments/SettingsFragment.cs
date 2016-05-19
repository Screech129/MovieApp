using Android.Content;
using Android.OS;
using Android.Preferences;
using Core;
using Model;
using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.XamarinAndroid;
using System;
using System.IO;
using System.Threading.Tasks;

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
            foreach (var key in PreferenceManager.GetDefaultSharedPreferences(Activity).All.Keys)
            {
                Preference pref = FindPreference(key);
                if (pref.GetType() == typeof(ListPreference))
                {
                    var listPref = (ListPreference)pref;
                    pref.Summary = listPref.Entry;
                }
                else
                {
                    pref.Summary = PreferenceManager.GetDefaultSharedPreferences(Activity).GetString(key, "");
                }
            }
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
            await RefreshDatabase();

            var prefEditor = sharedPreferences.Edit();
            prefEditor.PutString(key, sharedPreferences.GetString(key, ""));
            prefEditor.Commit();


        }

        private static async Task RefreshDatabase ()
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");

            SQLiteConnectionString connString = new SQLiteConnectionString(dbPath, false);
            SQLiteConnectionWithLock conn = new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), connString);
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(() => conn);

            var helper = new MovieDbHelper(db);
            var tableExist = await helper.TableExists("Movies");
            var movieCount = 0;
            if (tableExist)
            {
                movieCount = await helper.CountRows<Movies>();
            }
            if (movieCount > 0)
            {
                var provider = new MovieProvider(db);
                Uri uri = Movies.ContentUri;
                await provider.DeleteRecords(uri, null, null);
            }
        }


    }
}

