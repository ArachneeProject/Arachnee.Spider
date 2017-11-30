using Newtonsoft.Json;

namespace Spider.Exports
{
    public class ArchiveEntry
    {
            [JsonProperty("adult")]
            public bool Adult { get; set; }

            [JsonProperty("id")]
            public ulong Id { get; set; }
    }
}