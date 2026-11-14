using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;
        private readonly IMoonService _moonService;

        //Planets are cached after the first successful load, to reduce multiple api call for same details
        private IReadOnlyCollection<Planet> _cachedPlanets;

        public PlanetService(HttpClientService httpClientService, IMoonService moonService)
        {
            _httpClientService = httpClientService;
            _moonService = moonService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            if (_cachedPlanets != null) return _cachedPlanets;

            var allPlanetsWithTheirMoons = new Collection<Planet>();

            Console.WriteLine(OutputString.LoadingPlanets);

            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsQueryParameters)
                .Result;

            //If the status code isn't 200-299, then the function returns an empty collection.
            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                return allPlanetsWithTheirMoons;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

            //The JSON converter can return a null object. 
            if (results?.Bodies == null) return allPlanetsWithTheirMoons;

            //All moons are loaded with ONE request and grouped by the planet they orbit.
            //Previously the API was called once per moon (about 400 sequential calls, done twice).
            var moonsByPlanet = _moonService.GetAllMoons()
                .Where(moon => moon.PlanetId != null)
                .ToLookup(moon => moon.PlanetId);

            foreach (var planetDto in results.Bodies)
            {
                allPlanetsWithTheirMoons.Add(new Planet(planetDto, moonsByPlanet[planetDto.Id]));
            }

            Console.WriteLine(OutputString.LoadedPlanets, allPlanetsWithTheirMoons.Count);
            _cachedPlanets = allPlanetsWithTheirMoons;
            return allPlanetsWithTheirMoons;
        }
    }
}
