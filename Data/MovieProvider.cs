using Core;
using Core.Ioc;
using Model;
using SQLite.Net;
using SQLite.Net.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Core
{

    public class MovieProvider
    {
        private MovieDbHelper movieHelper;
        public const int MoviesUri = 100;
        public const int FavoritesUri = 101;
        public const int MovieFromId = 102;
        public const int FavoriteFromId = 103;
        private readonly SQLiteAsyncConnection _db;

        public MovieProvider (SQLiteAsyncConnection conn)
            : base()
        {
            _db = conn;
        }

        private async Task<List<Favorites>> GetFavoriteMovies (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {

            var Movies = _db.Table<Favorites>();
            var movies = await Movies.ToListAsync().ContinueWith(m => m);
            return movies.Result;
        }

        private AsyncTableQuery<Movies> GetAllMovies (Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            var Movies = _db.Table<Movies>();
            return Movies;
        }


        private static int MatchUri (Uri uri)
        {
            var parsedUri = uri.Parse();
            if (parsedUri.Id.HasValue)
            {
                if (parsedUri.Table == "movies")
                {
                    return MovieFromId;
                }
                else if (parsedUri.Table == "favorites")
                {
                    return FavoriteFromId;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (parsedUri.Table == "movies")
                {
                    return MoviesUri;
                }
                else if (parsedUri.Table == "favorites")
                {
                    return FavoritesUri;
                }
                else
                {
                    return -1;
                }
            }

        }
        public async Task<int> DeleteRecords (Uri uri, string selection, string[] selectionArgs)
        {
            var deletedRows = 0;

            var match = MatchUri(uri);
            if (selection == null) selection = "1";

            switch (match)
            {
                case FavoritesUri:
                    deletedRows = await _db.DeleteAsync(selection);
                    break;
                case MoviesUri:
                    deletedRows = await _db.ExecuteAsync("DELETE FROM 'Movies'");
                    break;
                default:
                    throw new Exception("Unkown uri: " + uri);
            }

            return deletedRows;
        }

        public int DropTables<T> () where T : class
        {
            var deletedRows = 0;
            deletedRows = _db.DropTableAsync<T>().Result;

            return deletedRows;
        }


        public async Task<List<int>> Insert<T> (Uri uri, List<T> values) where T : BaseColumns
        {
            var table = _db.Table<T>();

            List<int> ids = new List<int>();
            try
            {
                foreach (var value in values)
                {
                    await _db.InsertAsync(value).ContinueWith(t =>
                    {
                        if (value.Id > 0)
                        {
                            ids.Add(value.Id);
                        }
                    });
                }
                return ids;
            }
            catch (Exception ex)
            {
                var logger = Container.Resolve<ILogger>();
                logger.Log("InsertStatement", ex);
            }
            return ids;
        }

        public async Task<List<T>> Query<T> (Uri uri) where T : BaseColumns
        {
            List<T> returnQuery = null;
            var parsedUri = uri.Parse();
            var match = MatchUri(uri);
            switch (parsedUri.Table)
            {
                case "movies":
                    var movies = await ExecuteQuery<Movies>(parsedUri);
                    return movies.Cast<T>().ToList();
                case "favorites":
                    var favorites = await ExecuteQuery<Favorites>(parsedUri);
                    return favorites.Cast<T>().ToList();
                default:
                    break;
            }
            return returnQuery;
        }

        private async Task<List<T>> ExecuteQuery<T> (ParsedUri parsedUri) where T : BaseColumns
        {
            var table = _db.Table<T>();

            try
            {
                if (!parsedUri.Id.HasValue)
                {
                    return await table.ToListAsync();
                }
                else
                {
                    var movieId = parsedUri.Id.Value;
                    return table.Where(m => m.MovieId == movieId).ToListAsync().Result;
                }
            }
            catch (Exception ex)
            {
                var logger = Container.Resolve<ILogger>();
                logger.Log("InsertStatement", ex);
                throw;
            }

        }

        public int Update<T> (Uri uri, List<T> values, string selection, string[] selectionArgs)
        {
            var updatedRows = 0;
            var match = MatchUri(uri);

            switch (match)
            {
                case FavoritesUri:
                    updatedRows = _db.UpdateAsync(values).Result;
                    break;
                case MoviesUri:
                    updatedRows = _db.UpdateAsync(values).Result;
                    break;
                default:
                    throw new Exception("Unknown uri: " + uri);
            }
            return updatedRows;
        }
    }
}