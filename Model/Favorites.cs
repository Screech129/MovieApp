using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace Model
{
    [Table("Favorites")]
    public class Favorites : BaseColumns
    {
        public static Uri ContentUri = new Uri(BaseColumns.BaseContentUri, "favorites");
        public const string PathFavorites = "favorites";

        public static Uri BuildUri (long id)
        {
            return new Uri("content://" + ContentUri + "/" + id);
        }
    }
}
