using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebsiteScraper
{
    public static class HttpContentExtensions
    {
        public static Task ReadAsFileAsync(this HttpContent content, string filename, bool overwrite)
        {
            var path = Path.GetFullPath(filename);
            if (overwrite && File.Exists(filename))
            {
                throw new InvalidOperationException($"File {path} already exists.");
            }

            FileStream stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(stream).ContinueWith(copyTask =>
                {
                    stream.Close();
                });
            }
            catch
            {
                stream?.Close();

                throw;
            }
        }
    }
}