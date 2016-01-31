
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

namespace MovieApp
{
    public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public SettingsFragment()
        {
            
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.pref_general);
            // Create your fragment here
        }

        public override void OnResume()
        {
            base.OnResume();
            PreferenceManager.GetDefaultSharedPreferences(Activity).RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();
            PreferenceManager.GetDefaultSharedPreferences(Activity).RegisterOnSharedPreferenceChangeListener(this);

        }
            

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            Preference pref = FindPreference (key);
            if (pref.GetType () == typeof(ListPreference)) {
                var listPref = (ListPreference)pref;
                pref.Summary = listPref.Entry;
            } else {
                pref.Summary = sharedPreferences.GetString (key,"");
            }
            var prefEditor = sharedPreferences.Edit ();
            prefEditor.PutString (key,sharedPreferences.GetString (key,""));
            prefEditor.Commit ();
        }
    }
}

