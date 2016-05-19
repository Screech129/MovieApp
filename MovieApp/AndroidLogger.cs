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
using Core;

namespace MovieApp
{
    public class AndroidLogger : ILogger
    {
        public void Log (string title, Exception ex)
        {
            Android.Util.Log.Debug(title, ex.ToString());
        }
    }
}