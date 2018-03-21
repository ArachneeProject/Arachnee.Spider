using Spider;
using Spider.Archives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Runner
{
    class Program
    {
        private const int MaxBound = 100;
        private const LogLevel MinLogLevel = LogLevel.Info;

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
            Logger.Instance.MinLogLevel = MinLogLevel;

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

            var archiveManager = new ArchiveManager(archiveFolder);
            archiveManager.LoadIds(DateTime.UtcNow.AddDays(-2), entities);
            
            // init crawler
            if (args.Length < 1)
            {
                Logger.Instance.LogError("No api key was given as argument of the program.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var databasePath = Path.Combine(spiderFolder, "Database");
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
            }

            var crawler = new TmdbCrawler(args[0], databasePath);

            var chrono = Stopwatch.StartNew();
            foreach (var kvp in archiveManager.LoadedIds)
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += PrintProgress;

                crawler.CrawlEntities(kvp.Key, kvp.Value, MaxBound, progress).Wait();

                progress.ProgressChanged -= PrintProgress;
            }
            chrono.Stop();

            Console.WriteLine("Crawling done, it took " + chrono.Elapsed + " to complete.");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void PrintProgress(object sender, double e)
        {
            Logger.Instance.LogInfo("Progress: " + e * 100 + "%");
        }
    }
}