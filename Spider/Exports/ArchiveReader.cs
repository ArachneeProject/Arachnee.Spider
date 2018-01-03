using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Spider.ArachneeCore;

namespace Spider.Exports
{
    public class ArchiveReader
    {
        private readonly TmdbProxy _proxy;
        private const float MinPopularity = 0.01f;

        public ArchiveReader(TmdbProxy proxy)
        {
            _proxy = proxy;
        }

        public int CountLines(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return 0;
            }

            var lineCount = 0;
            int lineBreakByte = Convert.ToByte('\n');
            using (var stream = File.OpenRead(filePath))
            {
                int currentByte;
                do
                {
                    currentByte = stream.ReadByte();
                    if (currentByte == lineBreakByte)
                    {
                        lineCount++;
                        continue;
                    }
                } while (currentByte != -1);
            }
            return lineCount;
        }

        public IEnumerable<Entry> ReadMovies(string archiveJsonPath)
        {
            using (var streamReader = new StreamReader(archiveJsonPath))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var archiveEntry = JsonConvert.DeserializeObject<ArchiveEntry>(line);

                    if (archiveEntry.Adult)
                    {
                        Logger.Instance.LogMessage("Skipped adult movie " + archiveEntry.Id);
                        continue;
                    }

                    if (archiveEntry.Popularity < MinPopularity)
                    {
                        Logger.Instance.LogMessage("Skipped movie " + archiveEntry.Id + " because its popularity (" + archiveEntry.Popularity + ") is less than " + MinPopularity);
                        continue;
                    }

                    Entry entry;
                    try
                    {
                        entry = _proxy.GetMovie(archiveEntry.Id);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogException(e);
                        continue;
                    }

                    yield return entry;
                }
            }
        }
    }
}