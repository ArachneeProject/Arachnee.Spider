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
            var tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arachnee.Spider");

            Logger.Initialize(Path.Combine(tempPath, "logs.txt"));

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var downloader = new ArchiveDownloader();

            Console.WriteLine("Downloading...");

            var movieZipPath = downloader.DownloadMovies(DateTime.UtcNow.AddDays(-2), tempPath);

            Console.WriteLine("Done!");
            Console.WriteLine("Unzipping...");

            var movieIdsPath = Unzipper.Unzip(movieZipPath);

            Console.WriteLine("Unzipped!");

            

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
