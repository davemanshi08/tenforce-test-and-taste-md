using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Exceptions;
using Test_Taste_Console_Application.Domain.Services;
using Xunit;

namespace Test_Taste_Console_Application.Tests
{
    ///<summary>
    /// Tests the services against a fake HTTP handler, so no real API calls (or API key) are needed.
    ///</summary>
    public class PlanetServiceTests
    {
        private const string PlanetsJson =
            "{\"bodies\":[{\"id\":\"terre\",\"semimajorAxis\":149598023,\"avgTemp\":288},{\"id\":\"mars\",\"semimajorAxis\":227939200,\"avgTemp\":210},{\"id\":\"venus\",\"semimajorAxis\":108208475,\"avgTemp\":737}]}";

        private const string MoonsJson =
            "{\"bodies\":[" +
            "{\"id\":\"lune\",\"avgTemp\":250,\"aroundPlanet\":{\"planet\":\"terre\",\"rel\":\"https://api.le-systeme-solaire.net/rest/bodies/terre\"}}," +
            "{\"id\":\"phobos\",\"avgTemp\":233,\"aroundPlanet\":{\"planet\":\"mars\",\"rel\":\"https://api.le-systeme-solaire.net/rest/bodies/mars\"}}," +
            "{\"id\":\"deimos\",\"avgTemp\":0,\"aroundPlanet\":{\"planet\":\"mars\",\"rel\":\"https://api.le-systeme-solaire.net/rest/bodies/mars\"}}]}";

        public PlanetServiceTests()
        {
            //HttpClientService requires an API key; any value works with the fake handler.
            Environment.SetEnvironmentVariable(HttpClientSettings.ApiKeyEnvironmentVariable, "test-key");
        }

        private static HttpClientService CreateHttpClientService(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
            new HttpClientService(new HttpClient(new FakeHttpMessageHandler(respond)));

        private static HttpResponseMessage Json(string json) =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        [Fact]
        public void GetAllPlanets_JoinsMoonsToTheirPlanet_WithTwoApiCalls()
        {
            var requestCount = 0;
            var httpClientService = CreateHttpClientService(request =>
            {
                requestCount++;
                return Json(request.RequestUri.Query.Contains("bodyType") ? MoonsJson : PlanetsJson);
            });
            var planetService = new PlanetService(httpClientService, new MoonService(httpClientService));

            var planets = planetService.GetAllPlanets().ToList();

            Assert.Equal(3, planets.Count);
            Assert.Equal(new[] { "lune" }, planets.Single(p => p.Id == "terre").Moons.Select(m => m.Id));
            Assert.Equal(new[] { "phobos", "deimos" }, planets.Single(p => p.Id == "mars").Moons.Select(m => m.Id));
            Assert.False(planets.Single(p => p.Id == "venus").HasMoons());
            Assert.Equal(233f, planets.Single(p => p.Id == "mars").AverageMoonTemperature);
            Assert.Equal(288f, planets.Single(p => p.Id == "terre").AverageTemperatureKelvin);
            Assert.Equal(2, requestCount);
        }

        [Fact]
        public void GetAllPlanets_CalledTwice_UsesCachedData()
        {
            var requestCount = 0;
            var httpClientService = CreateHttpClientService(request =>
            {
                requestCount++;
                return Json(request.RequestUri.Query.Contains("bodyType") ? MoonsJson : PlanetsJson);
            });
            var planetService = new PlanetService(httpClientService, new MoonService(httpClientService));

            planetService.GetAllPlanets();
            planetService.GetAllPlanets();

            Assert.Equal(2, requestCount);
        }

        [Fact]
        public void GetAllPlanets_Unauthorized_ThrowsWithApiKeyHint()
        {
            var httpClientService = CreateHttpClientService(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var planetService = new PlanetService(httpClientService, new MoonService(httpClientService));

            var exception = Assert.Throws<SolarSystemApiException>(() => planetService.GetAllPlanets());

            Assert.Contains("401", exception.Message);
            Assert.Contains(HttpClientSettings.ApiKeyEnvironmentVariable, exception.Message);
        }

        [Fact]
        public void GetAllPlanets_InvalidJson_ThrowsSolarSystemApiException()
        {
            var httpClientService = CreateHttpClientService(_ => Json("this is not json"));
            var planetService = new PlanetService(httpClientService, new MoonService(httpClientService));

            Assert.Throws<SolarSystemApiException>(() => planetService.GetAllPlanets());
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                _respond = respond;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_respond(request));
            }
        }
    }
}
