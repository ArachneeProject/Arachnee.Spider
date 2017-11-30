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

                    yield return _proxy.GetMovie(archiveEntry.Id);
                }
            }
        }
    }
}