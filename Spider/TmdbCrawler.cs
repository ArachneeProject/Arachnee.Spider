using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.Collections;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.TvShows;

namespace Spider
{
    public class TmdbCrawler
    {
        private readonly TMDbClient _client;
        private readonly string _destinationFolder;

        public TmdbCrawler(string apiKey, string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                throw new DirectoryNotFoundException(destinationFolder);
            }

            _client = new TMDbClient(apiKey);
            _destinationFolder = destinationFolder;
        }
        
        public async Task CrawlEntities(Entity entity, List<int> ids, int maxBound, IProgress<double> progress)
        {
            Logger.Instance.LogInfo($"Crawling entities of type {entity}...");
            progress.Report(0);

            if (maxBound > ids.Count)
            {
                maxBound = ids.Count;
            }

            var entityFolder = Path.Combine(_destinationFolder, "Entities", entity.ToString());
            if (!Directory.Exists(entityFolder))
            {
                Directory.CreateDirectory(entityFolder);
            }

            double count = 0;
            foreach (var id in ids)
            {
                if (count > maxBound)
                {
                    break;
                }

                Logger.Instance.LogDebug($"Processing {entity} {id}...");
                progress.Report(count / maxBound);
                count++;

                string filePath = Path.Combine(entityFolder, id + ".json");
                if (File.Exists(filePath))
                {
                    Logger.Instance.LogDebug($"File {filePath} already exists.");
                    continue;
                }

                object crawledEntity = null;
                try
                {
                    switch (entity)
                    {
                        case Entity.Movie:
                            crawledEntity = await _client.GetMovieAsync(id, (MovieMethods) 4239);
                            break;

                        case Entity.Person:
                            crawledEntity = await _client.GetPersonAsync(id, (PersonMethods) 31);
                            break;

                        case Entity.TvSeries:
                            crawledEntity = await _client.GetTvShowAsync(id, (TvShowMethods) 127);
                            break;

                        case Entity.Collection:
                            crawledEntity = await _client.GetCollectionAsync(id, CollectionMethods.Images);
                            break;

                        case Entity.Keyword:
                            crawledEntity = await _client.GetKeywordAsync(id);
                            break;

                        default:
                            Logger.Instance.LogError($"Entity {entity} is not handled.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.LogException(e);
                }

                if (crawledEntity == null)
                {
                    continue;
                }

                var serialized = JsonConvert.SerializeObject(crawledEntity, Formatting.Indented);
                if (string.IsNullOrEmpty(serialized))
                {
                    continue;
                }

                File.WriteAllText(filePath, serialized);
            }
            
            progress.Report(1);
            Logger.Instance.LogInfo($"Crawling of enties {entity} done.");
        }

        public async Task CrawlLabels(Label label, IProgress<double> progress)
        {
            progress.Report(0);

            var labelsFolder = Path.Combine(_destinationFolder, "Labels", label.ToString());
            if (!Directory.Exists(labelsFolder))
            {
                Directory.CreateDirectory(labelsFolder);
            }

            string filePath = Path.Combine(labelsFolder, label + "s.json");
            if (File.Exists(filePath))
            {
                Logger.Instance.LogInfo($"File {filePath} already exists.");
                return;
            }

            object crawledLabels = null;
            try
            {
                switch (label)
                {
                    case Label.Job:
                        crawledLabels = await _client.GetJobsAsync();
                        break;
                    case Label.Genre:
                        Logger.Instance.LogWarning("Genre are not available (yet?).");
                        break;

                    default:
                        Logger.Instance.LogError($"Label {label} is not handled.");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
            }

            if (crawledLabels == null)
            {
                return;
            }

            var serialized = JsonConvert.SerializeObject(crawledLabels, Formatting.Indented);
            if (string.IsNullOrEmpty(serialized))
            {
                return;
            }

            File.WriteAllText(filePath, serialized);
        }
    }
}