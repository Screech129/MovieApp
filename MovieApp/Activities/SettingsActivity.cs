﻿
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
using MovieApp.Fragments;

namespace MovieApp.Activities
{
    [Activity(Label = "SettingsActivity")]			
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_detail);
            var fragTx = this.FragmentManager.BeginTransaction();

            fragTx.Replace(Resource.Id.movie_detail_container, new SettingsFragment()).Commit();
        }
    }
}

