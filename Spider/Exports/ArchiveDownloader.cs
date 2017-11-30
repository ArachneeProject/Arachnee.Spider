using System;
using System.IO;
using System.Net;
using RestSharp;
using Spider.Tmdb;

namespace Spider.Exports
{
    public class ArchiveDownloader
    {
        private readonly RestClient _client = new RestClient("http://files.tmdb.org/p/exports/");

        public string DownloadMovies(DateTime archiveDate, string destinationFolder)
        {
            if (archiveDate.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(nameof(archiveDate), $"{nameof(archiveDate)} is not UTC.");
            }

            if (archiveDate > DateTime.UtcNow)
            {
                throw new ArgumentException(nameof(archiveDate),
                    $"{nameof(archiveDate)} is not valid because it is in the future.");
            }

            if (!Directory.Exists(destinationFolder))
            {
                throw new DirectoryNotFoundException($"Folder doesn't exist at \"{destinationFolder}\"");
            }

            string resource = $"movie_ids_{archiveDate.Month:00}_{archiveDate.Day:00}_{archiveDate.Year:0000}.json.gz";
            
            var request = new RestRequest(resource, Method.GET);

            var response = _client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new FailedRequestException(_client.BuildUri(request).ToString(), response);
            }

            string filePath = Path.Combine(destinationFolder, resource);

            File.WriteAllBytes(filePath, response.RawBytes);

            return filePath;
        }
    }
}