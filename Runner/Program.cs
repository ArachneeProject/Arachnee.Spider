using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spider;
using Spider.ArachneeCore;
using Spider.Exports;
using Spider.Serialization;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var spiderFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Arachnee.Spider");
            var logFilePath = Path.Combine(spiderFolder, DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_spider.log");
            Logger.Initialize(logFilePath);

            Console.WriteLine("Log file at " + logFilePath);

            if (!Directory.Exists(spiderFolder))
            {
                Directory.CreateDirectory(spiderFolder);
            }

            var downloader = new ArchiveDownloader();

            var movieZipPath = downloader.DownloadMovies(DateTime.UtcNow.AddDays(-2), spiderFolder);

            var movieIdsPath = Unzipper.Unzip(movieZipPath);

            var proxy = new TmdbProxy();

            var reader = new ArchiveReader(proxy);
            var entries = reader.ReadMovies(movieIdsPath);

            string outputFilePath = Path.Combine(spiderFolder, "output.json");

            var serializer = new HighPressureSerializer(outputFilePath);

            int i = 0;
            foreach (var entry in entries)
            {
                i++;
                if (i > 50)
                {
                    break;
                }

                var toCompress = new List<Connection>();
                foreach (var connection in entry.Connections)
                {
                    var connectedEntry = proxy.GetEntry(connection.ConnectedId);
                    if (string.IsNullOrEmpty(connectedEntry.MainImagePath))
                    {
                        continue;
                    }

                    toCompress.Add(connection);
                }

                if (toCompress.Count == 0)
                {
                    continue;
                }

                serializer.CompressAndWrite(entry.Id, toCompress);
            }

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}