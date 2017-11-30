using System;
using System.IO;
using Spider;
using Spider.Exports;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var spiderFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arachnee.Spider");
            var logFilePath = Path.Combine(spiderFolder, "logs.txt");
            Logger.Initialize(logFilePath);

            Console.WriteLine("Log file created at " + logFilePath);

            if (!Directory.Exists(spiderFolder))
            {
                Directory.CreateDirectory(spiderFolder);
            }

            var downloader = new ArchiveDownloader();
            
            var movieZipPath = downloader.DownloadMovies(DateTime.UtcNow.AddDays(-2), spiderFolder);
            
            var movieIdsPath = Unzipper.Unzip(movieZipPath);
            
            var reader = new ArchiveReader();
            var movies = reader.ReadMovieIds(movieIdsPath);

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
