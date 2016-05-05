using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;
using Android.Database.Sqlite;
using Java.Lang;
using System.IO;
using SQLite;

namespace MovieApp.Data
{
    [ContentProvider(new string[] { BaseColumns.ContentAuthority }, Exported = true, Syncable = true)]
    public class MovieProvider
    {
        private UriMatcher uriMatcher = BuildUriMatcher();
        private MovieDbHelper movieHelper;
        private static Context context = Application.Context;
        public const int Movies = 100;
        public const int Favorites = 101;
        public const int MovieFromId = 102;
        public const int FavoriteFromId = 103;
        public static string Authority = "com.silverlining.movieapp"; //context.GetString(Resource.String.content_authority);
        string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "movie.db3");
        
        public MovieProvider ()
            : base()
        { 

        }

        private TableQuery<MovieContract.FavoritesTable> GetFavoriteMovies (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            var db = new SQLiteConnection(dbPath);
            
            var moviesTable = db.Table<MovieContract.FavoritesTable>();
            var movies = moviesTable.OrderBy(m=>sortOrder).Select(m => m);
            var movies2 = from m in moviesTable
                          select projection;
            return movies;
        }

        private TableQuery<MovieContract.MoviesTable> GetAllMovies (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            var db = new SQLiteConnection(dbPath);
            var moviesTable = db.Table<MovieContract.MoviesTable>();
            var movies = moviesTable.OrderBy(m => sortOrder).Select(m => m);
            var movies2 = from m in moviesTable
                          select projection;
            return movies;
        }


        private static UriMatcher BuildUriMatcher ()
        {
            var matcher = new UriMatcher(UriMatcher.NoMatch);

            matcher.AddURI(Authority, MovieContract.PathFavorites, Favorites);
            matcher.AddURI(Authority, MovieContract.PathFavorites + "/#", FavoriteFromId);

            matcher.AddURI(Authority, "movies", Movies);
            matcher.AddURI(Authority, "movies" + "/#", MovieFromId);

            return matcher;
        }
        public int Delete (Uri uri, string selection, string[] selectionArgs)
        {
            var db = new SQLiteConnection(dbPath);
            var deletedRows = 0;

            var match = uriMatcher.Match(uri);
            if (selection == null) selection = "1";

            switch (match)
            {
                case Favorites:
                    deletedRows = db.Delete(typeof(MovieContract.FavoritesTable));
                    break;
                case Movies:
                    deletedRows = db.Delete(typeof(MovieContract.MoviesTable));
                    break;
                default:
                    throw new UnsupportedOperationException("Unkown uri: " + uri);
            }

            if (deletedRows > 0 || selection == null) context.ContentResolver.NotifyChange(uri, null);
            db.Close();
            return deletedRows;
        }

        public string GetType (Uri uri)
        {
            int match = uriMatcher.Match(uri);

            switch (match)
            {
                case Favorites:
                    return MovieContract.FavoritesTable.ContentType;
                case Movies:
                    return MovieContract.MoviesTable.ContentType;
                case FavoriteFromId:
                    return MovieContract.FavoritesTable.ContentItemType;
                case MovieFromId:
                    return MovieContract.FavoritesTable.ContentItemType;
                default:
                    throw new UnsupportedOperationException("Unknown uri: " + uri);
            }
        }

        public Uri Insert<T> (Uri uri, List<T> values)
        {
            var db = new SQLiteConnection(dbPath);
            Uri returnUri;
            int match = uriMatcher.Match(uri);
            switch (match)
            {
                case Favorites:
                    {
                        long _id = db.Insert(values);
                        if (_id > 0) returnUri = MovieContract.FavoritesTable.BuildUri(_id);
                        else throw new SQLException("Failed to insert row into " + uri);
                        break;
                    }
                case Movies:
                    {
                        long _id = db.Insert(values);
                        if (_id > 0) returnUri = MovieContract.MoviesTable.BuildUri(_id);
                        else throw new SQLException("Failed to insert row into " + uri);
                        break;
                    }

                default:
                    throw new UnsupportedOperationException("Unknown uri: " + uri);
                
            }
            db.Close();
            context.ContentResolver.NotifyChange(uri, null);
            return returnUri;
        }

        public int BulkInsert (Uri uri, ContentValues[] values)
        {
            var db = new SQLiteConnection(dbPath);
            int match = uriMatcher.Match(uri);

            switch (match)
            {
                case Movies:
                    db.BeginTransaction();
                    int returnCount = 0;
                    try
                    {
                        foreach (ContentValues value in values)
                        {
                            long _id = db.Insert(value);
                            if (_id != -1) returnCount++;
                        }
                        
                    }
                    finally
                    {
                        db.Close();
                    }
                    context.ContentResolver.NotifyChange(uri, null);
                    return returnCount;
                default:
                    return 0;
            }
            
        }

        public bool OnCreate ()
        {
            movieHelper = new MovieDbHelper(context);
            return true;
        }

        public BaseTableQuery Query (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            BaseTableQuery returnQuery = null;
            var match = uriMatcher.Match(uri);
            switch (match)
            {
                case Favorites:
                    {
                        returnQuery = GetFavoriteMovies(uri, projection, null, null, sortOrder);
                        break;
                    }
                case FavoriteFromId:
                    {
                        returnQuery = GetFavoriteMovies(uri, projection, " WHERE movie_id = ? ", selectionArgs, sortOrder);
                        break;
                    }
                case Movies:
                    {
                        returnQuery = GetAllMovies(uri, projection, null, null, sortOrder);
                        break;
                    }
                case MovieFromId:
                    {
                        returnQuery = GetAllMovies(uri, projection, " WHERE movie_id = ? ", selectionArgs,sortOrder);
                        break;
                    }
                default:
                    break;
            }
            context.ContentResolver.NotifyChange(uri, null);
            return returnQuery;
        }

        public int Update (Uri uri, ContentValues values, string selection, string[] selectionArgs)
        {
            var db = new SQLiteConnection(dbPath);
            var updatedRows = 0;
            var match = uriMatcher.Match(uri);

            switch (match)
            {
                case Favorites:
                    updatedRows = db.Update(values, typeof(MovieContract.FavoritesTable));
                    break;
                case Movies:
                    updatedRows = db.Update(values,typeof(MovieContract.MoviesTable));
                    break;
                default:
                    throw new UnsupportedOperationException("Unknown uri: " + uri);
            }
            if (updatedRows > 0 || selection == null) context.ContentResolver.NotifyChange(uri, null);
            return updatedRows;
        }
    }
}