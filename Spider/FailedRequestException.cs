using System;
using RestSharp;

namespace Spider
{
    public class FailedRequestException : Exception
    {
        public IRestResponse Response { get; }

        public FailedRequestException(string request, IRestResponse response) 
            : base($"Request \"{request}\" failed with code {(int)response.StatusCode} {response.StatusCode}: {response.ErrorMessage}")
        {
            Response = response;
        }
    }
}