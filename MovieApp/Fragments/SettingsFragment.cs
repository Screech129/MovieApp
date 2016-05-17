
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
using SQLite;
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
            var db = new SQLiteAsyncConnection(dbPath);
            var tableExist = await MovieDbHelper.TableExists(db, "Movies");
            var movieCount = 0;
            if (tableExist)
            {
                movieCount = await db.Table<MovieContract.MoviesTable>().CountAsync();
            }
            if (movieCount > 0)
            {
                var provider = new MovieProvider();
                Android.Net.Uri uri = MovieContract.MoviesTable.ContentUri;
                await provider.DeleteRecords(uri, null, null);
            }
        }


    }
}

