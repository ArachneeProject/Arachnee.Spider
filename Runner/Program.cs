using System;
using System.IO;
using Spider;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arachnee.Spider");

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var downloader = new ArchiveDownloader();

            Console.WriteLine("Downloading...");

            var movieZipPath = downloader.GetMovies(DateTime.UtcNow.AddDays(-2), tempPath);

            Console.WriteLine("Done!");
            Console.WriteLine("Unzipping...");

            var movieIdsPath = Unzipper.Unzip(movieZipPath);

            Console.WriteLine("Unzipped!");

            using (var streamReader = new StreamReader(movieIdsPath))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();

                }
            }

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
