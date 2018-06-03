using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace Spider.Archives
{
    public class ArchiveManager
    {
        private readonly RestClient _client = new RestClient("http://files.tmdb.org/p/exports/");
        private readonly string _destinationFolder;
        private readonly Logger _logger;

        public Dictionary<EntityType, string> DownloadedArchivePaths { get; } = new Dictionary<EntityType, string>();

        public Dictionary<EntityType, string> UnzippedArchivePaths { get; } = new Dictionary<EntityType, string>();

        public Dictionary<EntityType, List<int>> LoadedIds { get; } = new Dictionary<EntityType, List<int>>();

        public bool ExcludeAdultIds { get; set; } = true;

        public double MinPopularity { get; set; } = 0.1;

        public ArchiveManager(string destinationFolder, Logger logger)
        {
            if (!Directory.Exists(destinationFolder))
            {
                throw new DirectoryNotFoundException($"Folder doesn't exist at \"{destinationFolder}\"");
            }

            _destinationFolder = destinationFolder;
            _logger = logger;
        }

        public void LoadIds(DateTime archiveDate, ICollection<EntityType> entities)
        {
            var chrono = Stopwatch.StartNew();

            _logger.LogInfo("Downloading...");
            Download(archiveDate, entities);
            _logger.LogInfo("Download took " + chrono.Elapsed);

            _logger.LogInfo("Unzipping...");
            foreach (var kvp in DownloadedArchivePaths)
            {
                var unzippedFile = UnzipDownloadedArchives(kvp.Value);
                UnzippedArchivePaths.Add(kvp.Key, unzippedFile);
            }

            _logger.LogInfo("Unzip took " + chrono.Elapsed);

            _logger.LogInfo("Loading ids...");
            foreach (var kvp in UnzippedArchivePaths)
            {
                var ids = ReadIds(kvp.Value);
                LoadedIds.Add(kvp.Key, ids);
            }

            _logger.LogInfo("Load took " + chrono.Elapsed);
            chrono.Stop();
        }
        
        private void Download(DateTime archiveDate, ICollection<EntityType> entities)
        {
            if (archiveDate.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(nameof(archiveDate), $"{nameof(archiveDate)} is not UTC.");
            }

            if (archiveDate > DateTime.UtcNow)
            {
                throw new ArgumentException(nameof(archiveDate), $"{nameof(archiveDate)} is not valid because it is in the future.");
            }

            foreach (var entity in entities)
            {
                string resourceName = ToResourceName(entity.ToString());
                var address = $"{resourceName}_ids_{archiveDate.Month:00}_{archiveDate.Day:00}_{archiveDate.Year:0000}.json.gz";
                var downloadedArchive = Download(address);
                DownloadedArchivePaths.Add(entity, downloadedArchive);
            }
        }

        private string ToResourceName(string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        private string Download(string resource)
        {
            string filePath = Path.Combine(_destinationFolder, resource);
            if (File.Exists(filePath))
            {
                _logger.LogInfo("No need to re-download " + resource);
                return filePath;
            }

            var request = new RestRequest(resource, Method.GET);

            _logger.LogInfo("Downloading " + resource + "...");

            var response = _client.Execute(request);

            _logger.LogInfo("Downloading done.");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new FailedRequestException(_client.BuildUri(request).ToString(), response);
            }

            File.WriteAllBytes(filePath, response.RawBytes);

            _logger.LogInfo("Archive file created at " + filePath);

            return filePath;
        }

        private string UnzipDownloadedArchives(string gzipFilePath)
        {
            if (!File.Exists(gzipFilePath))
            {
                _logger.LogError($"Archive file was not found at \"{gzipFilePath}\"");
                return string.Empty;
            }

            _logger.LogInfo("Unzipping " + gzipFilePath);

            string decompressedFilePath;

            var fileToDecompress = new FileInfo(gzipFilePath);
            using (var originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                decompressedFilePath =
                    currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (var decompressedFileStream = File.Create(decompressedFilePath))
                {
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }

            _logger.LogInfo($"File unzipped at {decompressedFilePath}");

            return decompressedFilePath;
        }

        private List<int> ReadIds(string jsonArchivePath)
        {
            var result = new List<int>();

            using (var streamReader = new StreamReader(jsonArchivePath))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var archiveEntry = JsonConvert.DeserializeObject<ArchiveEntry>(line);

                    if (archiveEntry.Adult && ExcludeAdultIds)
                    {
                        _logger.LogDebug($"Skipped adult: {archiveEntry}");
						continue;
                    }

                    if (archiveEntry.Popularity >= 0 //check if type really has a Popularity at all
                        && archiveEntry.Popularity < MinPopularity)
                    {
                        _logger.LogDebug($"Skipped low popularity: {archiveEntry.Id}");
						continue;
                    }

                    result.Add(archiveEntry.Id);
                }
            }

            return result;
        }
    }
}