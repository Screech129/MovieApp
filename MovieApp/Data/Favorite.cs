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
using Realms;
using Uri = Android.Net.Uri;

namespace MovieApp.Data
{

    public class Favorite:RealmObject
    {
        public const string PathMovies = "movies";
        public const string PathFavorites = "favorites";
        public const string ContentAuthority = "com.silverlining.movieapp";
        public static Uri BaseContentUri = Uri.Parse("content//" + ContentAuthority);

        public Favorite ()
        {

        }
        [ObjectId]
        public int Id { get; set; }
        public int MovieId { get; set; }

        public string MovieTitle { get; set; }

        public string Plot { get; set; }

        public string PosterPath { get; set; }

        public DateTimeOffset ReleaseDate { get; set; }

        public string Reviews { get; set; }

        public string Trailers { get; set; }

        public float VoteAverage { get; set; }

        public static Uri ContentUri = BaseContentUri.BuildUpon().AppendPath(PathFavorites).Build();
        public static string ContentType = ContentResolver.CursorDirBaseType + "/" + ContentAuthority + "/" + PathFavorites;
        public static string ContentItemType = ContentResolver.CursorItemBaseType + "/" + ContentAuthority + "/" + PathFavorites;


        public static Uri BuildUri (long id)
        {
            return ContentUris.WithAppendedId(ContentUri, id);
        }

    }
}