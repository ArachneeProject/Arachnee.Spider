using Newtonsoft.Json;

namespace Spider.Tmdb.TmdbObjects
{
    public class SpokenLanguage
    {
        [JsonProperty("iso_639_1")]
        public string LanguageIsoCode { get; set; }
        public string Name { get; set; }
    }
}