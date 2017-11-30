using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;
using Spider.Tmdb.TmdbObjects;

namespace Spider.Tmdb
{
    public class TmdbClient
    {
        private readonly RestClient _client = new RestClient("https://api.themoviedb.org/3/");
        
        /// <summary>
        /// Get the primary informations about a movie.
        /// </summary>
        public TmdbMovie GetMovie(ulong tmdbMovieId)
        {
            var request = new RestRequest("movie/{id}", Method.GET)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddUrlSegment("id", tmdbMovieId.ToString());
            request.AddQueryParameter("append_to_response", "credits,images");
            request.AddQueryParameter("api_key", Constant.ApiKey);

            var response = ExecuteRequest(request);

            var movie = JsonConvert.DeserializeObject<TmdbMovie>(response, TmdbJsonSettings.Instance);
            if (movie.Id == default(ulong))
            {
                throw new ArgumentException($"Movie id \"{tmdbMovieId}\" didn't return any result.");
            }

            return movie;
        }

        /// <summary>
        /// Get the primary person details.
        /// </summary>
        public TmdbPerson GetPerson(ulong tmdbPersonId)
        {
            var request = new RestRequest("person/{id}", Method.GET)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddUrlSegment("id", tmdbPersonId.ToString());
            request.AddQueryParameter("append_to_response", "combined_credits,images");
            request.AddQueryParameter("api_key", Constant.ApiKey);

            var response = ExecuteRequest(request);

            var person = JsonConvert.DeserializeObject<TmdbPerson>(response, TmdbJsonSettings.Instance);
            if (person.Id == default(ulong))
            {
                throw new ArgumentException($"Person id \"{tmdbPersonId}\" didn't return any result.");
            }

            return person;
        }
        
        private string ExecuteRequest(IRestRequest request)
        {
            var response = _client.Execute(request);
            Thread.Sleep(250); // ensure no more than 40 request per 10s are executed.

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new FailedRequestException(_client.BuildUri(request).ToString(), response);
            }

            return response.Content;
        }
    }
}