using System;
using RestSharp;

namespace Spider.Tmdb
{
    public class FailedRequestException : Exception
    {
        public IRestResponse Response { get; }

        public FailedRequestException(string request, IRestResponse response) 
            : base($"Request \"{request}\" failed with code {response.StatusCode}: {response.ErrorMessage}{response.ErrorException}{response.Content}")
        {
            Response = response;
        }
    }
}