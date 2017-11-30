using System.Collections.Generic;

namespace Spider.Tmdb.TmdbObjects
{
    public class Credits
    {
        public List<Cast> Cast { get; set; }
        public List<Crew> Crew { get; set; }
    }
}