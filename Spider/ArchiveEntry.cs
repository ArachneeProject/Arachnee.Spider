using Newtonsoft.Json;

namespace Spider
{
    public class ArchiveEntry
    {
            [JsonProperty("adult")]
            public bool Adult { get; set; }

            [JsonProperty("id")]
            public long Id { get; set; }
    }
}