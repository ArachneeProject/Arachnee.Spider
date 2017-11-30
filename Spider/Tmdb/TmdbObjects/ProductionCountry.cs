using Newtonsoft.Json;

namespace Spider.Tmdb.TmdbObjects
{
    public class ProductionCountry
    {
        [JsonProperty("iso_639_1")]
        public string LanguageIsoCode { get; set; }
        public string Name { get; set; }
    }
}