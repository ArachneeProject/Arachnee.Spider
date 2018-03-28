using Spider;
using Spider.Archives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Runner
{
    class Program
    {
        private const int DefaultMaxBound = 50;
        private const LogLevel DefaultMinLogLevel = LogLevel.Info;
        private const bool DefaultExcludeAdultIds = true;
        private const double DefaultMinPopularity = 0.1;

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
            _logger = new Logger(logFilePath) {MinLogLevel = DefaultMinLogLevel};

            Console.WriteLine("Log file at " + logFilePath);
            
            // entity types user input
            Console.WriteLine($"Available options are: {string.Join(",", Enum.GetNames(typeof(EntityType)))}.");
            Console.WriteLine("Press enter to download everything, or write what to load (separated by commas)...");

            var input = Console.ReadLine();

            var entityTypes = new List<EntityType>();
            if (string.IsNullOrEmpty(input))
            {
                var allTypes = Enum.GetValues(typeof(EntityType));
                entityTypes.AddRange(allTypes.Cast<EntityType>());
            }
            else
            {
                var choices = input.Replace(" ", "").Split(',');
                foreach (var choice in choices)
                {
                    if (Enum.TryParse(choice, true, out EntityType entiy))
                    {
                        entityTypes.Add(entiy);
                    }
                }
            }
            
            if (entityTypes.Count == 0)
            {
                _logger.LogInfo("Nothing to do. Press any key...");
                Console.ReadKey();
                return;
            }

            _logger.LogInfo($"Asked to download {string.Join(",", entityTypes)}");

            // items count user input
            Console.WriteLine($"Press enter to download {DefaultMaxBound} itmes of each, or write 0 to get them all, or write the number of items you want...");

            input = Console.ReadLine();

            int maxBound;
            if (string.IsNullOrEmpty(input))
            {
                maxBound = DefaultMaxBound;
            }
            else
            {
                if (!int.TryParse(input, out maxBound))
                {
                    _logger.LogError($"{input} is not a number. Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }

            // init archive
            var archiveFolder = Path.Combine(spiderFolder, "archives");
            if (!Directory.Exists(archiveFolder))
            {
                Directory.CreateDirectory(archiveFolder);
            }

            var archiveManager = new ArchiveManager(archiveFolder, _logger)
            {
                MinPopularity = DefaultMinPopularity,
                ExcludeAdultIds = DefaultExcludeAdultIds
            };

            archiveManager.LoadIds(DateTime.UtcNow.AddDays(-2), entityTypes);
            
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

                crawler.CrawlEntities(kvp.Key, kvp.Value, maxBound, progress);

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