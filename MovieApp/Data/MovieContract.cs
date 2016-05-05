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
    public class MovieContract
    {
        public const string PathMovies = "movies";
        public const string PathFavorites = "favorites";

        public MovieContract ()
        {

        }


        [Table("Movies")]
        public class MoviesTable : BaseColumns
        {
            public MoviesTable ()
                : base()
            {

            }

            [NotNull]
            [Indexed(Name ="MovieIdTitle",Order=1,Unique=true)]
            public int MovieId { get; set; }

            [NotNull]
            [Indexed(Name ="MovieIdTitle",Order=2,Unique=true)]
            public string MovieTitle { get; set; }

            [NotNull]
            public string Plot { get; set; }

            [NotNull]
            public string PosterPath { get; set; }

            [NotNull]
            public int ReleaseDate { get; set; }

            [NotNull]
            public string Reviews { get; set; }

            [NotNull]
            public string Trailers { get; set; }

            [NotNull]
            public decimal VoteAverage { get; set; }

            public static Uri ContentUri = BaseContentUri.BuildUpon().AppendPath(PathMovies).Build();
            public static string ContentType = ContentResolver.CursorDirBaseType + "/" + ContentAuthority + "/" + PathMovies;
            public static string ContentItemType = ContentResolver.CursorItemBaseType + "/" + ContentAuthority + "/" + PathMovies;

            public static Uri BuildUri (long id)
            {
                return ContentUris.WithAppendedId(ContentUri, id);
            }

        }

        [Table("Favorites")]
        public class FavoritesTable : BaseColumns
        {
            public FavoritesTable ()
                : base()
            {
            }

            [NotNull]
            public int MovieId { get; set; }

            [NotNull]
            public string MovieTitle { get; set; }

            [NotNull]
            public string Plot { get; set; }

            [NotNull]
            public string PosterPath { get; set; }

            [NotNull]
            public int ReleaseDate { get; set; }

            [NotNull]
            public string Reviews { get; set; }

            [NotNull]
            public string Trailers { get; set; }

            [NotNull]
            public int VoteAverage { get; set; }

            public static Uri ContentUri = BaseContentUri.BuildUpon().AppendPath(PathFavorites).Build();
            public static string ContentType = ContentResolver.CursorDirBaseType + "/" + ContentAuthority + "/" + PathFavorites;
            public static string ContentItemType = ContentResolver.CursorItemBaseType + "/" + ContentAuthority + "/" + PathFavorites;


            public static Uri BuildUri (long id)
            {
                return ContentUris.WithAppendedId(ContentUri, id);
            }




        }
    }
}