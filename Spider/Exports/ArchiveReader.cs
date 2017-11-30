using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Spider.Tmdb;
using Spider.Tmdb.TmdbObjects;

namespace Spider.Exports
{
    public class ArchiveReader
    {
        private TmdbClient _client = new TmdbClient();

        public IEnumerable<TmdbMovie> LoadMovies(string archiveJsonPath)
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

                    TmdbMovie movie;
                    try
                    {
                        movie = _client.GetMovie(archiveEntry.Id);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.
                    }
                    
                }
            }
        }
    }
}