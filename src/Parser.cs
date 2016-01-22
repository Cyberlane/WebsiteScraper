using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsQuery;

namespace WebsiteScraper
{
    public class Parser
    {
        public ParseResult Parse(string filePath, Func<string, bool> validator)
        {
            Console.WriteLine($"Parsing file for additional files: {filePath}");

            string content;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            var cq = CQ.Create(content);
            var hrefResults = cq.Find("[href]").Selection
                .Select(x => x.Attributes["href"])
                .Distinct()
                .Where(validator)
                .Select(x => new Uri(x));

            var srcResults = cq.Find("[src]").Selection
                .Select(x => x.Attributes["src"])
                .Distinct()
                .Where(validator)
                .Select(x => new Uri(x));

            var metaImage = cq.Find("meta[og:image]").Selection
                .Select(x => x.Attributes["content"])
                .Distinct()
                .Where(validator)
                .Select(x => new Uri(x));

            var resources = new List<Uri>();
            resources.AddRange(hrefResults);
            resources.AddRange(srcResults);
            resources.AddRange(metaImage);

            return new ParseResult
            {
                Resources = resources
            };
        }
    }
}