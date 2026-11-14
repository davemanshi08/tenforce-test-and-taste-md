using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Exceptions;

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
            Client.Timeout = TimeSpan.FromSeconds(HttpClientSettings.TimeoutInSeconds);
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

        ///<summary>
        /// will send a GET request and return the response body.
        /// API failures will return through a SolarSystemApiException with a proper message
        ///</summary>
        public string GetContent(string requestUri)
        {
            HttpResponseMessage response;
            try
            {
                response = Client.GetAsync(requestUri).GetAwaiter().GetResult();
            }
            catch (HttpRequestException exception)
            {
                throw new SolarSystemApiException($"{ExceptionMessage.ApiNotReachableMessage}{exception.Message}", exception);
            }
            catch (TaskCanceledException exception)
            {
                throw new SolarSystemApiException(ExceptionMessage.ApiTimeoutMessage, exception);
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = $"{ExceptionMessage.ApiRequestFailedMessage}{(int)response.StatusCode} {response.ReasonPhrase}.";

                //401/403 almost always means a missing or invalid API key.
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    message += $" {ExceptionMessage.CheckApiKeyMessage}{HttpClientSettings.ApiKeyEnvironmentVariable}";
                }

                throw new SolarSystemApiException(message);
            }

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
    }
}
