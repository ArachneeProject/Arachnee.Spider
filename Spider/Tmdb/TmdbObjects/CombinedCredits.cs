using System.Collections.Generic;

namespace Spider.Tmdb.TmdbObjects
{
    public class CombinedCredits
    {
        public List<Cast> Cast { get; set; }
        public List<Cast> Crew { get; set; }
    }
}