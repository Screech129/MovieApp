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
using Realms;
using System.Threading.Tasks;
using Android.Util;

namespace MovieApp.Data
{
    [ContentProvider(new string[] { Movie.ContentAuthority }, Exported = true, Syncable = true)]
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
        Realm realm;
        public MovieProvider ()
            : base()
        {
            realm = Realm.GetInstance();
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
        public bool DeleteRecords (Uri uri, string selection, string[] selectionArgs)
        {
            var match = MatchUri(uri);
            var hasError = false;
            switch (match)
            {
                case Favorites:
                    using (var trans = realm.BeginWrite())
                    {
                        try
                        {
                            realm.RemoveAll<Favorite>();
                            trans.Commit();
                        }
                        catch (System.Exception)
                        {
                            hasError = true;
                            throw;
                        }

                    }
                    break;
                case Movies:
                    using (var trans = realm.BeginWrite())
                    {
                        try
                        {
                            realm.RemoveAll<Movie>();
                            trans.Commit();
                        }
                        catch (System.Exception)
                        {
                            hasError = true;
                            throw;
                        }

                    }
                    break;
                default:
                    throw new UnsupportedOperationException("Unkown uri: " + uri);
            }
            return hasError;
        }

        public string GetType (Uri uri)
        {
            int match = MatchUri(uri);

            switch (match)
            {
                case Favorites:
                    return Favorite.ContentType;
                case Movies:
                    return Movie.ContentType;
                case FavoriteFromId:
                    return Favorite.ContentItemType;
                case MovieFromId:
                    return Favorite.ContentItemType;
                default:
                    throw new UnsupportedOperationException("Unknown uri: " + uri);
            }
        }

        public Uri Insert<T> (Uri uri, List<T> values)
        {
            Uri returnUri = null;
            int match = MatchUri(uri);
            switch (match)
            {
                case Favorites:
                    {
                        long _id = 0;
                        try
                        {
                            var value = (Favorite)(object)values[0];
                            try
                            {
                                realm.Write(() =>
                                {
                                    var favorite = realm.CreateObject<Favorite>();
                                    favorite = value;
                                    _id = value.Id;
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
                            var value = (Movie)(object)values[0];
                            realm.Write(() =>
                            {
                                var movie = realm.CreateObject<Movie>();
                                movie = value;
                                _id = value.Id;
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

        public int BulkInsert (Uri uri, List<Movie> values)
        {
            int match = MatchUri(uri);

            switch (match)
            {
                case Movies:
                    int returnCount = 0;
                    try
                    {
                        long _id = 0;

                        using (var trans = realm.BeginWrite())
                        {
                            foreach (Movie value in values)
                            {
                                var movie = realm.CreateObject<Movie>();
                                movie = value;
                                _id = value.Id;
                                if (_id > 0) returnCount++;
                            }
                        };
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

        public RealmResults<Favorite> QueryFavorites (Uri uri, string[] projection, string selection, string sortOrder)
        {
            var match = MatchUri(uri);
            RealmResults<Favorite> returnQuery = null;
            switch (match)
            {
                case Favorites:
                    {
                        returnQuery = realm.All<Favorite>();
                        break;
                    }
                case FavoriteFromId:
                    {
                        returnQuery = (RealmResults<Favorite>)realm.All<Favorite>().Where(f => f.MovieId == int.Parse(selection));
                        break;
                    }
                default:
                    break;
            }
            context.ContentResolver.NotifyChange(uri, null);
            return returnQuery;
        }

        public RealmResults<Movie> QueryMovies (Uri uri, string[] projection, string selection, string sortOrder)
        {
            var match = MatchUri(uri);
            RealmResults<Movie> returnQuery = null;
            switch (match)
            {
                case Favorites:
                    {
                        returnQuery = realm.All<Movie>();
                        break;
                    }
                case FavoriteFromId:
                    {
                        returnQuery = (RealmResults<Movie>)realm.All<Movie>().Where(f => f.MovieId == int.Parse(selection));
                        break;
                    }
                default:
                    break;
            }
            context.ContentResolver.NotifyChange(uri, null);
            return returnQuery;
        }


    }
}