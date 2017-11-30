using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Spider.ArachneeCore;
using Spider.Tmdb;

namespace Spider.Exports
{
    public class ArchiveReader
    {
        private readonly TmdbProxy _proxy = new TmdbProxy();

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