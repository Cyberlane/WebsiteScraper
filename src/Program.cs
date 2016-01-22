using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace WebsiteScraper
{
    class Program
    {
        static Dictionary<Uri, bool> _resources = new Dictionary<Uri, bool>();

        static void Main(string[] args)
        {
            Console.WriteLine("#####################");
            Console.WriteLine("#  Website Scraper  #");
            Console.WriteLine("#####################");
            Console.WriteLine();
            var websiteUri = GetWebsiteUri();
            if (!Directory.Exists(websiteUri.Host))
            {
                Directory.CreateDirectory(websiteUri.Host);
            }
            _resources.Add(websiteUri, false);

            while (_resources.Any())
            {
                var next = _resources.FirstOrDefault(x => !x.Value);
                if (next.Key == null)
                {
                    continue;
                }

                _resources[next.Key] = true;
                FetchUri(next.Key);
            }

            Console.WriteLine();
            Console.WriteLine($"Completed downloading website: {websiteUri.Host}");
            Console.ReadKey();
        }

        static void FetchUri(Uri uri)
        {
            Console.WriteLine($"Fetching: {uri}");
            var httpClient = new HttpClient();
            var urlParser = new UrlParser();

            httpClient.GetAsync(uri).ContinueWith(request =>
            {
                var response = request.Result;
                response.EnsureSuccessStatusCode();
                var urlResults = urlParser.Parse(uri);
                var folder = CreateDirectories(urlResults.Folders, uri.Host);
                var filePath = Path.Combine(folder, urlResults.Filename);

                if (response.Content.Headers.ContentType.MediaType == "text/html")
                {
                    filePath += ".html";
                }

                response.Content.ReadAsFileAsync(filePath, false).ContinueWith(readTask =>
                {
                    Console.WriteLine($"Download Complete: {uri}");
                    _resources.Remove(uri);
                });
            });
        }

        static string CreateDirectories(string[] folders, string parentFolder = null)
        {
            var folder = folders.First();
            if (parentFolder != null)
            {
                folder = Path.Combine(parentFolder, folder);
            }

            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Creating folder: {folder}");
                Directory.CreateDirectory(folder);
            }

            var remainingFolders = folders.Skip(1).ToArray();
            if (remainingFolders.Length > 0)
            {
                return CreateDirectories(folders.Skip(1).ToArray(), folder);
            }

            return folder;
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
