using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Test_Taste_Console_Application.Constants;

namespace Test_Taste_Console_Application.Domain.Services
{
    ///<summary>
    /// A service to create the HttpClient. 
    ///</summary>
    public class HttpClientService
    {
        public HttpClient Client { get; }

        public HttpClientService(HttpClient client)
        {
            //The HTTP client is configured in the constructor.
            Client = client;
            Client.BaseAddress = new Uri(UriPath.BaseUri);
            Client.DefaultRequestHeaders.Accept.Add(new
                MediaTypeWithQualityHeaderValue(HttpClientSettings.JsonType));

            //The API requires a Bearer token on every request (else its returning 401 Unauthorized).
            //Stored key as environment variable and reading the key from environment variable.
            var apiKey = Environment.GetEnvironmentVariable(HttpClientSettings.ApiKeyEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    $"{ExceptionMessage.ApiKeyMissing}{HttpClientSettings.ApiKeyEnvironmentVariable}");
            }

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(HttpClientSettings.BearerScheme, apiKey);
        }
    }
}
