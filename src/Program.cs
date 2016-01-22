using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("#####################");
            Console.WriteLine("#  Website Scraper  #");
            Console.WriteLine("#####################");
            Console.WriteLine();
            var uri = GetWebsiteUri();
            Console.WriteLine("Fetching: " + uri);
            Console.ReadKey();
        }

        private static Uri GetWebsiteUri()
        {
            while (true)
            {
                Console.Write("Enter website URL: ");
                var url = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(url))
                {
                    Console.WriteLine("Error: Please enter a URL.");
                    continue;
                }

                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    Console.WriteLine("Error: Invalid URL.");
                    continue;
                }

                if (!uri.IsWellFormedOriginalString())
                {
                    Console.WriteLine("Error: Invalid URL.");
                    continue;
                }

                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    return uri;
                }

                Console.WriteLine("Error: Prefix your URL with HTTP:// or HTTPS://");
            }
        }
    }
}
