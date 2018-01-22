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
        private const float MinPopularity = 0.1f;

        public event Action<ulong> SkippedId;

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

        public IEnumerable<Entry> Read<TEntry>(string archiveJsonPath) where TEntry : Entry
        {
            using (var streamReader = new StreamReader(archiveJsonPath))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var archiveEntry = JsonConvert.DeserializeObject<ArchiveEntry>(line);

                    if (archiveEntry.Adult)
                    {
                        Logger.Instance.LogMessage($"Skipped adult {typeof(TEntry).Name} {archiveEntry.Id}");
                        continue;
                    }

                    if (archiveEntry.Popularity < MinPopularity)
                    {
                        Logger.Instance.LogMessage($"Skipped {typeof(TEntry).Name} {archiveEntry.Id} because its popularity ({archiveEntry.Popularity} is less than {MinPopularity}).");
                        SkippedId?.Invoke(archiveEntry.Id);
                        continue;
                    }

                    Entry entry;
                    try
                    {
                        entry = _proxy.GetEntry($"{typeof(TEntry).Name}-{archiveEntry.Id}");
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