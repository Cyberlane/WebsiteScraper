using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace WebsiteScraper
{
    class Program
    {
        static readonly ConcurrentQueue<Uri> Queue = new ConcurrentQueue<Uri>();
        static readonly List<Uri> ProcessedList = new List<Uri>();
        private static int InProgress = 0;

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
            Queue.Enqueue(websiteUri);

            while (Queue.Any() || InProgress > 0)
            {
                Uri nextUri;
                if (Queue.TryDequeue(out nextUri))
                {
                    InProgress++;
                    ProcessedList.Add(nextUri);
                    FetchUri(nextUri);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Completed downloading website: {websiteUri.Host}");
            Console.WriteLine($" > {ProcessedList.Count} files downloaded");
            Console.WriteLine(" > Press any key to exit");
            Console.ReadKey();
        }

        static async void FetchUri(Uri uri)
        {
            Console.WriteLine($"Fetching: {uri}");
            var httpClient = new HttpClient();
            var urlParser = new UrlParser();
            var parser = new Parser();

            var response = await httpClient.GetAsync(uri);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine($"Error '{response.ReasonPhrase}' when trying to fetch: {uri}");
                InProgress--;
                return;
            }

            var urlResults = urlParser.Parse(uri);
            var folder = CreateDirectories(urlResults.Folders, uri.Host);
            var filePath = Path.Combine(folder, urlResults.Filename);

            if (filePath == uri.Host)
            {
                filePath = Path.Combine(filePath, "index");
            }

            if (!Path.HasExtension(filePath))
            {
                switch (response.Content.Headers.ContentType.MediaType)
                {
                    case "text/html":
                        filePath += ".html";
                        break;
                    case "application/rss+xml":
                        filePath += ".xml";
                        break;
                    case "application/json":
                        filePath += ".json";
                        break;
                }
            }


            FileStream stream = null;
            try
            {
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(stream);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error '{ex.Message}' when trying to fetch: {uri}");
            }
            finally
            {
                stream?.Close();
            }

            Console.WriteLine($"Download Complete: {uri}");

            if (response.Content.Headers.ContentType.MediaType == "text/html")
            {
                var parseResults = parser.Parse(filePath, href => href.StartsWith(uri.AbsoluteUri));
                var resources = parseResults.Resources.Where(x => !ProcessedList.Contains(x) && !Queue.Contains(x));
                foreach (var newUri in resources)
                {
                    Queue.Enqueue(newUri);
                }
            }

            InProgress--;
        }

        static string CreateDirectories(string[] folders, string parentFolder = null)
        {
            if (folders.Length == 0)
            {
                return parentFolder;
            }

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

    public enum DownloadStatus
    {
        Pending = 0,
        Downloading = 1,
        Complete = 2,
        Error = 3
    }
}
