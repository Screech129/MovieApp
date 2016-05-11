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
using System.Threading.Tasks;
using Android.Util;

namespace MovieApp.Data
{
    [ContentProvider(new string[] { BaseColumns.ContentAuthority }, Exported = true, Syncable = true)]
    public class MovieProvider
    {
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

        private async Task<List<MovieContract.FavoritesTable>> GetFavoriteMovies (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            var db = new SQLiteAsyncConnection(dbPath);

            var moviesTable = db.Table<MovieContract.FavoritesTable>();
            var movies = await moviesTable.ToListAsync().ContinueWith(m => m);
            return movies.Result;
        }

        private AsyncTableQuery<MovieContract.MoviesTable> GetAllMovies (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            var db = new SQLiteAsyncConnection(dbPath);
            var moviesTable = db.Table<MovieContract.MoviesTable>();
            return moviesTable;
        }


        private static int MatchUri (Uri uri)
        {
            var uriPath = uri.PathSegments;
            if (uriPath.Count == 3)
            {
                if (uriPath[2] == "movies")
                {
                    return Movies;
                }
                else
                {
                    return Favorites;
                }
            }
            else if (uriPath.Count > 3)
            {
                if (uriPath[2] == "movies")
                {
                    return MovieFromId;
                }
                else
                {
                    return FavoriteFromId;
                }
            }
            return -1;
        }
        public async Task<int> DeleteRecords (Uri uri, string selection, string[] selectionArgs)
        {
            var db = new SQLiteAsyncConnection(dbPath);
            var deletedRows = 0;

            var match = MatchUri(uri);
            if (selection == null) selection = "1";

            switch (match)
            {
                case Favorites:
                    deletedRows = await db.DeleteAsync(selection);
                    break;
                case Movies:
                    deletedRows = await db.ExecuteAsync("DELETE FROM 'Movies'");
                    break;
                default:
                    throw new UnsupportedOperationException("Unkown uri: " + uri);
            }

            if (deletedRows > 0 || selection == null) context.ContentResolver.NotifyChange(uri, null);
            return deletedRows;
        }

        public int DropTables (Uri uri, string selection, string[] selectionArgs)
        {
            var db = new SQLiteAsyncConnection(dbPath);
            var deletedRows = 0;

            var match = MatchUri(uri);
            if (selection == null) selection = "1";

            switch (match)
            {
                case Favorites:
                    deletedRows = db.DropTableAsync<MovieContract.FavoritesTable>().Result;
                    break;
                case Movies:
                    deletedRows = db.DropTableAsync<MovieContract.MoviesTable>().Result;
                    break;
                default:
                    throw new UnsupportedOperationException("Unkown uri: " + uri);
            }

            if (deletedRows > 0 || selection == null) context.ContentResolver.NotifyChange(uri, null);
            return deletedRows;
        }

        public string GetType (Uri uri)
        {
            int match = MatchUri(uri);

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

        public async Task<Uri> Insert<T> (Uri uri, List<T> values)
        {
            var db = new SQLiteAsyncConnection(dbPath);
            Uri returnUri = null;
            int match = MatchUri(uri);
            switch (match)
            {
                case Favorites:
                    {
                        long _id = 0;
                        try
                        {
                            var value = (MovieContract.FavoritesTable)(object)values[0];
                            try
                            {
                                await db.InsertAsync(value).ContinueWith(t =>
                                {
                                    _id = value.Id;
                                    if (_id > 0) returnUri = MovieContract.FavoritesTable.BuildUri(_id);
                                    else throw new SQLException("Failed to insert row into " + uri);
                                    context.ContentResolver.NotifyChange(uri, null);
                                    return returnUri;
                                });
                            }
                            catch (System.Exception ex)
                            {
                                Log.Debug("SQLException", ex.ToString());
                                throw;
                            }
                           

                        }
                        catch (System.Exception ex)
                        {
                            Log.Debug("SqlInsert", ex.Message);
                            throw;
                        }

                        break;

                    }
                case Movies:
                    {
                        long _id = 0;
                        try
                        {
                            var value = (MovieContract.MoviesTable)(object)values[0];
                            await db.InsertAsync(value).ContinueWith(t =>
                            {
                                _id = value.Id;
                                if (_id > 0) returnUri = MovieContract.MoviesTable.BuildUri(_id);
                                else throw new SQLException("Failed to insert row into " + uri);
                                context.ContentResolver.NotifyChange(uri, null);
                                return returnUri;
                            });

                        }
                        catch (System.Exception ex)
                        {
                            Log.Debug("SqlInsert", ex.Message);
                            throw;
                        }

                        break;
                    }

                default:
                    throw new UnsupportedOperationException("Unknown uri: " + uri);
            }
            return returnUri;
        }

        public async Task<int> BulkInsert (Uri uri, List<MovieContract.MoviesTable> values)
        {
            var db = new SQLiteAsyncConnection(dbPath);
            var table = db.Table<MovieContract.MoviesTable>();
            int match = MatchUri(uri);

            switch (match)
            {
                case Movies:
                    int returnCount = 0;
                    try
                    {
                        long _id = -1;
                       await db.RunInTransactionAsync(async (SQLiteConnection conn) =>
                        {
                            foreach (MovieContract.MoviesTable value in values)
                            {
                                await db.InsertAsync(value).ContinueWith(t =>
                                {
                                    _id = t.Id;
                                    if (_id != -1) returnCount++;
                                });
                            }
                            conn.Close();
                        });

                        context.ContentResolver.NotifyChange(uri, null);
                        return returnCount;
                    }
                    catch (Java.Lang.Exception ex)
                    {
                        Log.Debug("BulkInsert", ex.Message);
                    }

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

        public async Task<List<MovieContract.FavoritesTable>> QueryFavorites (Uri uri, string[] projection, string selection, string sortOrder)
        {
            List<MovieContract.FavoritesTable> returnQuery = null;
            var match = MatchUri(uri);
            var db = new SQLiteAsyncConnection(dbPath);
            var moviesTable = db.Table<MovieContract.FavoritesTable>();
            switch (match)
            {
                case Favorites:
                    {
                        await moviesTable.ToListAsync();
                        break;
                    }
                case FavoriteFromId:
                    {
                        await moviesTable.Where(m => m.MovieId == int.Parse(selection)).ToListAsync();
                        break;
                    }
                default:
                    break;
            }
            context.ContentResolver.NotifyChange(uri, null);
            return returnQuery;
        }

        public async Task<List<MovieContract.MoviesTable>> QueryMovies (Uri uri, string[] projection, string selection, string sortOrder)
        {
            List<MovieContract.MoviesTable> returnQuery = null;
            var match = MatchUri(uri);
            var db = new SQLiteAsyncConnection(dbPath);
            var moviesTable = db.Table<MovieContract.MoviesTable>();
            switch (match)
            {
                case Movies:
                    {
                        returnQuery = await moviesTable.ToListAsync();
                        break;
                    }
                case MovieFromId:
                    {
                        returnQuery = await moviesTable.Where(m => m.MovieId == int.Parse(selection)).ToListAsync();
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
            var db = new SQLiteAsyncConnection(dbPath);
            var updatedRows = 0;
            var match = MatchUri(uri);

            switch (match)
            {
                case Favorites:
                    updatedRows = db.UpdateAsync(values).Result;
                    break;
                case Movies:
                    updatedRows = db.UpdateAsync(values).Result;
                    break;
                default:
                    throw new UnsupportedOperationException("Unknown uri: " + uri);
            }
            if (updatedRows > 0 || selection == null) context.ContentResolver.NotifyChange(uri, null);
            return updatedRows;
        }
    }
}