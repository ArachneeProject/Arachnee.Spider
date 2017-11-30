using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Spider.Tmdb;

namespace Spider.Exports
{
    public class ArchiveReader
    {
        private TmdbClient _client = new TmdbClient();

        public IEnumerable<ulong> ReadMovieIds(string archiveJsonPath)
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

                    yield return archiveEntry.Id;
                }
            }
        }
    }
}