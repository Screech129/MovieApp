using System;
using Android.Content;
using System.IO;
using SQLite;
using Android.Util;
using System.Threading.Tasks;

namespace MovieApp.Data
{
    internal class MovieDbHelper
    {
        public const int DatabaseVersion = 1;
        public const string DbName = "movie.db";
        public MovieDbHelper (Context context)
           
        {

        }

        public async Task CreateTable (Type tableToCreate)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"movie.db3");
            var db = new SQLiteAsyncConnection(dbPath);
            await db.CreateTablesAsync(CreateFlags.None, tableToCreate);
        }

        public void DeleteDatabase ()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "movie.db3");
            File.Delete(dbPath);
        }

        public async static Task<bool> TableExists (SQLiteAsyncConnection connection, string tableName)
        {
            try
            {
                
                const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
                return await connection.ExecuteScalarAsync<string>(cmdText,tableName) != null;
            }
            catch (Exception ex)
            {
                Log.Debug("SQLException", ex.Message);
                return false;
            }
            
        }
       
    }
}