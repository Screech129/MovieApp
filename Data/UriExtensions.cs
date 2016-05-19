using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class UriExtensions
    {
        public static ParsedUri Parse (this Uri uri)
        {
            return new UriParser(uri).Parse();
        }
    }
}
