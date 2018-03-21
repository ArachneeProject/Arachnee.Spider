using Newtonsoft.Json;

namespace Spider.Exports
{
    public class ArchiveEntry
    {
        [JsonProperty("adult")]
        public bool Adult { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("popularity")]
        public float Popularity { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("original_name")]
        public string OriginalName { get; set; }

        [JsonProperty("original_title")]
        public string OriginalTitle { get; set; }

        public override string ToString()
        {
            string res = Name ?? string.Empty;
            if (string.IsNullOrEmpty(res))
            {
                res = OriginalName ?? string.Empty;
            }

            if (string.IsNullOrEmpty(res))
            {
                res = OriginalTitle ?? string.Empty;
            }
            
            return res + "(" + Id + ")";
        }
    }
}