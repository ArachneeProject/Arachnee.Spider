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
        private const int MaxBound = 50;
        private const LogLevel MinLogLevel = LogLevel.Info;
        private const bool ExcludeAdultIds = true;
        private const double MinPopularity = 0.1;

        private static Logger _logger;

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
            _logger = new Logger(logFilePath) {MinLogLevel = MinLogLevel};

            Console.WriteLine("Log file at " + logFilePath);
            
            // user input
            Console.WriteLine($"Available options are: {string.Join(",", Enum.GetNames(typeof(EntityType)))}.");
            Console.WriteLine("Write what to load (separated by commas)...");

            var input = Console.ReadLine();
            var choices = input.Replace(" ", "").Split(',');

            var entities = new List<EntityType>();
            foreach (var choice in choices)
            {
                if (Enum.TryParse(choice, true, out EntityType entiy))
                {
                    entities.Add(entiy);
                }
            }

            if (entities.Count == 0)
            {
                _logger.LogInfo("Nothing to do. Press any key...");
                Console.ReadKey();
                return;
            }

            _logger.LogInfo($"Asked to download {string.Join(",", entities)}");

            // init archive
            var archiveFolder = Path.Combine(spiderFolder, "archives");
            if (!Directory.Exists(archiveFolder))
            {
                Directory.CreateDirectory(archiveFolder);
            }

            var archiveManager = new ArchiveManager(archiveFolder, _logger)
            {
                MinPopularity = MinPopularity,
                ExcludeAdultIds = ExcludeAdultIds
            };

            archiveManager.LoadIds(DateTime.UtcNow.AddDays(-2), entities);
            
            // init crawler
            if (args.Length < 1)
            {
                _logger.LogError("No api key was given as argument of the program.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var databasePath = Path.Combine(spiderFolder, "Database");
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
            }

            var crawler = new TmdbCrawler(args[0], databasePath, _logger);

            var chrono = Stopwatch.StartNew();
            foreach (var kvp in archiveManager.LoadedIds)
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += PrintProgress;

                crawler.CrawlEntities(kvp.Key, kvp.Value, MaxBound, progress);

                progress.ProgressChanged -= PrintProgress;
            }
            chrono.Stop();

            _logger.Dispose();

            Console.WriteLine("Crawling done, it took " + chrono.Elapsed + " to complete.");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void PrintProgress(object sender, double e)
        {
            _logger.LogInfo("Progress: " + e * 100 + "%");
        }
    }
}