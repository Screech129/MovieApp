using System;
using SQLite.Net.Attributes;

namespace Model
{
    public class BaseColumns
    {
        public static Uri BaseContentUri = new Uri("content://com.silverlining.movieapp");
        public const string ContentAuthority = "com.silverlining.movieapp";

        [PrimaryKey]
        [AutoIncrement]
        [Column("_id")]
        public int Id { get; set; }

        [NotNull]
        [Indexed(Name = "MovieIdTitle", Order = 1, Unique = true)]
        public int MovieId { get; set; }

        [NotNull]
        [Indexed(Name = "MovieIdTitle", Order = 2, Unique = true)]
        public string MovieTitle { get; set; }

        [NotNull]
        public string Plot { get; set; }

        [NotNull]
        public string PosterPath { get; set; }

        [NotNull]
        public DateTime ReleaseDate { get; set; }

        [NotNull]
        public string Reviews { get; set; }

        public string Trailers { get; set; }

        [NotNull]
        public decimal VoteAverage { get; set; }
    }
}
