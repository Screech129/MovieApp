using System;
using Android.Content;
using System.IO;
using SQLite;

namespace MovieApp.Data
{
    internal class MovieDbHelper
    {
        public const int DatabaseVersion = 1;
        public const string DbName = "movie.db";
        public MovieDbHelper (Context context)
           
        {

        }

        public void CreateDatabase (Type tableToCreate)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"movie.db3");
            var db = new SQLiteConnection(dbPath);
            db.CreateTable(tableToCreate, CreateFlags.None);
        }
       
    }
}