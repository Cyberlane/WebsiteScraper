using System;
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
            var results = cq.Find("[href]").Selection
                .Select(x => x.Attributes["href"])
                .Where(validator)
                .Distinct()
                .Select(x => new Uri(x));

            return new ParseResult
            {
                Resources = results
            };
        }
    }
}