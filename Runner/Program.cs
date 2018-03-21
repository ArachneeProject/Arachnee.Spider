using Spider;
using Spider.Exports;
using System;
using System.Collections.Generic;
using System.IO;

namespace Runner
{
    class Program
    {
        private static int _progress = 0;

        static void Main(string[] args)
        {
            // init folder
            var spiderFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arachnee.Spider");
            if (!Directory.Exists(spiderFolder))
            {
                Directory.CreateDirectory(spiderFolder);
            }

            // init logger
            var now = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            var logFilePath = Path.Combine(spiderFolder, now + "_spider.log");
            Logger.Initialize(logFilePath);
            Logger.Instance.MinLogLevel = LogLevel.Info;

            Console.WriteLine("Log file at " + logFilePath);
            
            // user input
            Console.WriteLine($"Available options are: {string.Join(",", Enum.GetNames(typeof(Entity)))}.");
            Console.WriteLine("Write what to load (separated by commas)...");

            var input = Console.ReadLine();
            var choices = input.Replace(" ", "").Split(',');

            var entities = new List<Entity>();
            foreach (var choice in choices)
            {
                if (Enum.TryParse(choice, true, out Entity entiy))
                {
                    entities.Add(entiy);
                }
            }

            Logger.Instance.LogInfo($"Asked to download {string.Join(",", entities)}");

            // init archive
            var archiveFolder = Path.Combine(spiderFolder, "archives");
            if (!Directory.Exists(archiveFolder))
            {
                Directory.CreateDirectory(archiveFolder);
            }

            var downloader = new ArchiveManager(archiveFolder);
            downloader.LoadIds(DateTime.UtcNow.AddDays(-2), entities);
            

            //var proxy = new TmdbProxy();

            //var reader = new ArchiveReader(proxy);
            //int entriesCount = reader.CountLines(idsPath);
            //reader.SkippedId += CountSkippedId;

            //IEnumerable<Entry> entries;
            //if (input == MovieChoice)
            //{
            //    entries = reader.Read<Movie>(idsPath);
            //}
            //else if (input == TvSeriesChoice)
            //{
            //    entries = reader.Read<TvSeries>(idsPath);
            //}
            //else
            //{
            //    Logger.Instance.LogError(input + " not handled.");
            //    return;
            //}
            

            //string outputFilePath = Path.Combine(spiderFolder, now + "_output.spdr");
            
            //var serializer = new HighPressureSerializer(outputFilePath);
            
            //var chrono = Stopwatch.StartNew();

            //int threshold = 0;

            //foreach (var entry in entries)
            //{
            //    _progress++;
            //    if (_progress > threshold)
            //    {
            //        float progress = (float)_progress / entriesCount * 100;
            //        Logger.Instance.LogInfo($"{_progress}/{entriesCount} ({progress:##0.000}%) - elapsed: {chrono.Elapsed}");

            //        threshold = _progress + 100;
            //    }

            //    if (string.IsNullOrEmpty(entry.MainImagePath))
            //    {
            //        Logger.Instance.LogDebug("Skip " + entry + " because it has no image.");
            //        continue;
            //    }
                
            //    var connectionsToCompress = new List<Connection>();
            //    foreach (var connection in entry.Connections.Where(c => c.Type != ConnectionType.Crew))
            //    {
            //        Logger.Instance.LogDebug(entry + " :: " + connection.Label + " :: " + connection.ConnectedId);
            //        connectionsToCompress.Add(connection);
            //    }

            //    if (connectionsToCompress.Count == 0)
            //    {
            //        continue;
            //    }

            //    serializer.CompressAndWrite(entry.Id, connectionsToCompress);
            //}

            Console.WriteLine("Job done, press any key");
            Console.ReadKey();
        }

        private static void CountSkippedId(ulong obj)
        {
            _progress++;
        }
    }
}