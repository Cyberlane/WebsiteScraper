using System;
using System.Linq;

namespace WebsiteScraper
{
    public class UrlParser
    {
        public UrlParseResult Parse(Uri uri)
        {
            var cleanSegments = uri.Segments.Select(x => x.Replace("/", string.Empty)).ToArray();

            var result = new UrlParseResult
            {
                Folders = cleanSegments.Take(uri.Segments.Length - 1).Skip(1).ToArray(),
                Filename = cleanSegments.Last()
            };

            return result;
        }
    }
}