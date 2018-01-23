using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string MovieChoice = "movies";
        private const string TvSeriesChoice = "tv";

        private static int _progress = 0;

        static void Main(string[] args)
        {
            var spiderFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arachnee.Spider");
            var now = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            var logFilePath = Path.Combine(spiderFolder, now + "_spider.log");
            Logger.Initialize(logFilePath);

            Console.WriteLine("Log file at " + logFilePath);

            if (!Directory.Exists(spiderFolder))
            {
                Directory.CreateDirectory(spiderFolder);
            }

            Console.WriteLine($"Write \"{MovieChoice}\" or \"{TvSeriesChoice}\" to choose what to download:");
            var choice = Console.ReadLine();
            Logger.Instance.LogMessage("Asked to download " + choice);

            var downloader = new ArchiveDownloader();

            string zipPath;
            if (choice == MovieChoice)
            {
                zipPath = downloader.Download<Movie>(DateTime.UtcNow.AddDays(-2), spiderFolder);
            }
            else if (choice == TvSeriesChoice)
            {
                zipPath = downloader.Download<TvSeries>(DateTime.UtcNow.AddDays(-2), spiderFolder);
            }
            else
            {
                Logger.Instance.LogError(choice + " not handled.");
                return;
            }

            var idsPath = Unzipper.Unzip(zipPath);
            
            var proxy = new TmdbProxy();

            var reader = new ArchiveReader(proxy);
            int entriesCount = reader.CountLines(idsPath);
            reader.SkippedId += CountSkippedId;

            IEnumerable<Entry> entries;
            if (choice == MovieChoice)
            {
                entries = reader.Read<Movie>(idsPath);
            }
            else if (choice == TvSeriesChoice)
            {
                entries = reader.Read<TvSeries>(idsPath);
            }
            else
            {
                Logger.Instance.LogError(choice + " not handled.");
                return;
            }
            

            string outputFilePath = Path.Combine(spiderFolder, now + "_output.spdr");
            
            var serializer = new HighPressureSerializer(outputFilePath);
            
            var chrono = Stopwatch.StartNew();

            int threshold = 0;

            foreach (var entry in entries)
            {
                _progress++;
                if (_progress > threshold)
                {
                    float progress = (float)_progress / entriesCount * 100;
                    Logger.Instance.LogMessage($"{_progress}/{entriesCount} ({progress:##0.000}%) - elapsed: {chrono.Elapsed}");

                    threshold = _progress + 100;
                }

                if (string.IsNullOrEmpty(entry.MainImagePath))
                {
                    Logger.Instance.LogDebug("Skip " + entry + " because it has no image.");
                    continue;
                }
                
                var connectionsToCompress = new List<Connection>();
                foreach (var connection in entry.Connections.Where(c => c.Type != ConnectionType.Crew))
                {
                    Logger.Instance.LogDebug(entry + " :: " + connection.Label + " :: " + connection.ConnectedId);
                    connectionsToCompress.Add(connection);
                }

                if (connectionsToCompress.Count == 0)
                {
                    continue;
                }

                serializer.CompressAndWrite(entry.Id, connectionsToCompress);
            }

            Console.WriteLine("Job done, press any key");
            Console.ReadKey();
        }

        private static void CountSkippedId(ulong obj)
        {
            _progress++;
        }
    }
}