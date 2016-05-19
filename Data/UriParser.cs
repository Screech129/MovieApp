using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class UriParser
    {
        private Uri uri;

        public UriParser (Uri uri)
        {
            this.uri = uri;  
        }

        public ParsedUri Parse ()
        {
            try
            {
                var uriSegments = uri.AbsoluteUri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();
                var id = 0;
                if (int.TryParse(uriSegments[0], out id))
                {
                    return new ParsedUri()
                    {
                        Id = id,
                        Table = uriSegments[1].ToLower()
                    };
                }
                else
                {
                    return new ParsedUri()
                    {
                        Table = uriSegments[0].ToLower()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ParsedUri()
                {
                    IsOk = false,
                    Message = ex.ToString()
                };
                throw;
            }
          

        }

    }

    public class ParsedUri:Response
    {
        public ParsedUri ()
        {

        }

        public int? Id { get; set; }
        public string Table { get; set; }

    }

    public class Response
    {
        public Response ()
        {
            IsOk = true;
        }

        public bool IsOk { get; set; }
        public string Message { get; set; }
    }
}
