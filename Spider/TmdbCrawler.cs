using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly Logger _logger;

        public TmdbCrawler(string apiKey, string destinationFolder, Logger logger)
        {
            if (!Directory.Exists(destinationFolder))
            {
                throw new DirectoryNotFoundException(destinationFolder);
            }

            _client = new TMDbClient(apiKey);
            _destinationFolder = destinationFolder;
            _logger = logger;
        }
        
        public void CrawlEntities(EntityType entityType, List<int> ids, int maxBound, IProgress<double> progress)
        {
            _logger.LogInfo($"Crawling entities of type {entityType}...");
            
            var entitiesFolder = Path.Combine(_destinationFolder, "Entities", entityType.ToString());
            if (!Directory.Exists(entitiesFolder))
            {
                Directory.CreateDirectory(entitiesFolder);
            }
            
            var alreadyCrawledIdsFilePath = Path.Combine(entitiesFolder, $"{entityType.ToString()}_crawled_ids.json");
            if (!File.Exists(alreadyCrawledIdsFilePath))
            {
                File.WriteAllText(alreadyCrawledIdsFilePath, string.Empty);
            }

            var alreadyCrawledIds = new HashSet<int>();
            using (var reader = new StreamReader(alreadyCrawledIdsFilePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (int.TryParse(line, out var crawledId))
                    {
                        alreadyCrawledIds.Add(crawledId);
                    }
                }
            }

            var entityFilePath = Path.Combine(entitiesFolder, $"{entityType.ToString()}.json");
            if (!File.Exists(entityFilePath))
            {
                File.WriteAllText(entityFilePath, string.Empty);
            }

            double count = 0;
            if (maxBound > ids.Count)
            {
                maxBound = ids.Count;
            }

            using (var entityWriter = new StreamWriter(entityFilePath, append:true))
            {
                using (var idWriter = new StreamWriter(alreadyCrawledIdsFilePath, append:true))
                {
                    foreach (var id in ids)
                    {
                        if (count > maxBound)
                        {
                            break;
                        }

                        _logger.LogDebug($"Processing {entityType} {id}...");
                        progress.Report(count / maxBound);
                        count++;
                        
                        if (alreadyCrawledIds.Contains(id))
                        {
                            _logger.LogDebug($"{entityType} {id} was already crawled.");
                            continue;
                        }

                        object crawledEntity = null;
                        try
                        {
                            switch (entityType)
                            {
                                case EntityType.Movie:
                                    crawledEntity = _client.GetMovieAsync(id, (MovieMethods)4239).Result;
                                    break;

                                case EntityType.Person:
                                    crawledEntity = _client.GetPersonAsync(id, (PersonMethods)31).Result;
                                    break;

                                case EntityType.TvSeries:
                                    crawledEntity = _client.GetTvShowAsync(id, (TvShowMethods)127).Result;
                                    break;

                                case EntityType.Collection:
                                    crawledEntity = _client.GetCollectionAsync(id, CollectionMethods.Images).Result;
                                    break;

                                case EntityType.Keyword:
                                    crawledEntity = _client.GetKeywordAsync(id).Result;
                                    break;

                                default:
                                    _logger.LogError($"EntityType {entityType} is not handled.");
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogException(e);
                        }

                        if (crawledEntity == null)
                        {
                            continue;
                        }

                        var serialized = JsonConvert.SerializeObject(crawledEntity, Formatting.None);
                        if (string.IsNullOrEmpty(serialized))
                        {
                            continue;
                        }

                        entityWriter.WriteLine(serialized);
                        alreadyCrawledIds.Add(id);
                        idWriter.WriteLine(id);
                    }
                }
            }

            
            
            progress.Report(1);
            _logger.LogInfo($"Crawling of enties {entityType} done.");
        }

        public void CrawlLabels(Label label, IProgress<double> progress)
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
                _logger.LogInfo($"File {filePath} already exists.");
                return;
            }

            object crawledLabels = null;
            try
            {
                switch (label)
                {
                    case Label.Job:
                        crawledLabels = _client.GetJobsAsync().Result;
                        break;
                    case Label.Genre:
                        _logger.LogWarning("Genre are not available (yet?).");
                        break;

                    default:
                        _logger.LogError($"Label {label} is not handled.");
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
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