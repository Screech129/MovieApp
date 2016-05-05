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
using Uri = Android.Net.Uri;
using SQLite;

namespace MovieApp.Data
{
   public class BaseColumns
    {
       
        public BaseColumns ()
        {
           
        }

        [PrimaryKey,AutoIncrement,Column("_id")]
        public int Id { get; set; }

        public const string ContentAuthority = "com.silverlining.movieapp";
        public static Uri BaseContentUri = Uri.Parse("content//" + ContentAuthority);


       

    }
}