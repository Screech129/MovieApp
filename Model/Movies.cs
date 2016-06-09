using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace Model
{
    [Table("Movies")]
    public class Movies : BaseColumns
    {

        [NotNull]
        public bool IsFavorite { get; set; }

        public static Uri ContentUri = new Uri(BaseContentUri, "movies");
        public const string PathMovies = "movies";

        public static Uri BuildUri (long id)
        {
            return new Uri(ContentUri+ "/" +id);
        }
    }
}
