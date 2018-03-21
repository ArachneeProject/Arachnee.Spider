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

        public Dictionary<Entity, string> DownloadedArchivePaths { get; } = new Dictionary<Entity, string>();

        public Dictionary<Entity, string> UnzippedArchivePaths { get; } = new Dictionary<Entity, string>();

        public Dictionary<Entity, List<int>> LoadedIds { get; } = new Dictionary<Entity, List<int>>();

        public bool ExcludeAdultIds { get; set; } = true;

        public double MinPopularity { get; set; } = 0.1;

        public ArchiveManager(string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                throw new DirectoryNotFoundException($"Folder doesn't exist at \"{destinationFolder}\"");
            }

            _destinationFolder = destinationFolder;
        }

        public void LoadIds(DateTime archiveDate, ICollection<Entity> entities)
        {
            var chrono = Stopwatch.StartNew();

            Logger.Instance.LogInfo("Downloading...");
            Download(archiveDate, entities);
            Logger.Instance.LogInfo("Download took " + chrono.Elapsed);

            Logger.Instance.LogInfo("Unzipping...");
            foreach (var kvp in DownloadedArchivePaths)
            {
                var unzippedFile = UnzipDownloadedArchives(kvp.Value);
                UnzippedArchivePaths.Add(kvp.Key, unzippedFile);
            }

            Logger.Instance.LogInfo("Unzip took " + chrono.Elapsed);

            Logger.Instance.LogInfo("Loading ids...");
            foreach (var kvp in UnzippedArchivePaths)
            {
                var ids = ReadIds(kvp.Value);
                LoadedIds.Add(kvp.Key, ids);
            }

            Logger.Instance.LogInfo("Load took " + chrono.Elapsed);
            chrono.Stop();
        }
        
        private void Download(DateTime archiveDate, ICollection<Entity> entities)
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
                Logger.Instance.LogInfo("No need to re-download " + resource);
                return filePath;
            }

            var request = new RestRequest(resource, Method.GET);

            Logger.Instance.LogInfo("Downloading " + resource + "...");

            var response = _client.Execute(request);

            Logger.Instance.LogInfo("Downloading done.");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new FailedRequestException(_client.BuildUri(request).ToString(), response);
            }

            File.WriteAllBytes(filePath, response.RawBytes);

            Logger.Instance.LogInfo("Archive file created at " + filePath);

            return filePath;
        }

        private string UnzipDownloadedArchives(string gzipFilePath)
        {
            if (!File.Exists(gzipFilePath))
            {
                Logger.Instance.LogError($"Archive file was not found at \"{gzipFilePath}\"");
                return string.Empty;
            }

            Logger.Instance.LogInfo("Unzipping " + gzipFilePath);

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

            Logger.Instance.LogInfo($"File unzipped at {decompressedFilePath}");

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
                        Logger.Instance.LogDebug($"Skipped adult: {archiveEntry}");
                    }

                    if (archiveEntry.Popularity < MinPopularity)
                    {
                        Logger.Instance.LogDebug($"Skipped low popularity: {archiveEntry.Id}");
                    }

                    result.Add(archiveEntry.Id);
                }
            }

            return result;
        }
    }
}