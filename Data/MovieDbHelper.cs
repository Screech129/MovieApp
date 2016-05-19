using System;
using System.Threading.Tasks;
using SQLite.Net.Async;
using Core.Ioc;
using System.Diagnostics;

namespace Core
{
    public class MovieDbHelper
    {
        public const int DatabaseVersion = 1;
        public const string DbName = "movie.db";
        private readonly SQLiteAsyncConnection _conn;
        public MovieDbHelper (SQLiteAsyncConnection conn)
        {
            _conn = conn;
        }
        public async Task CreateTable (Type tableToCreate)
        {
            await _conn.CreateTablesAsync(tableToCreate);
        }

        public async Task<int> CountRows<T> () where T : class
        {
            return await _conn.Table<T>().CountAsync();
        }

        public async Task<bool> TableExists (string tableName)
        {
            try
            {

                const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
                return await _conn.ExecuteScalarAsync<string>(cmdText, tableName) != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

        }

    }
}