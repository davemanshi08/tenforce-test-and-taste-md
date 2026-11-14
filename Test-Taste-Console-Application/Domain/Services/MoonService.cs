using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Exceptions;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class MoonService : IMoonService
    {
        private readonly HttpClientService _httpClientService;

        //Moons details are cached after the first successful load, to reduce multiple api call for same details
        private IReadOnlyCollection<Moon> _cachedMoons;

        public MoonService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public IEnumerable<Moon> GetAllMoons()
        {
            if (_cachedMoons != null) return _cachedMoons;

            Console.WriteLine(OutputString.LoadingMoons);

            //Errors are handled in HttpClientService and thrown as a SolarSystemApiException
            var content = _httpClientService.GetContent(UriPath.GetAllMoonsQueryParameters);
            JsonResult<MoonDto> results;
            try
            {
                results = JsonConvert.DeserializeObject<JsonResult<MoonDto>>(content);
            }
            catch (JsonException exception)
            {
                throw new SolarSystemApiException($"{ExceptionMessage.ApiInvalidResponseMessage}{exception.Message}", exception);
            }

            //The JSON converter can return a null object. 
            if (results?.Bodies == null) return new Collection<Moon>();

            var allMoons = new Collection<Moon>();
            foreach (MoonDto moonDto in results.Bodies)
            {
                allMoons.Add(new Moon(moonDto));
            }

            Console.WriteLine(OutputString.LoadedMoons, allMoons.Count);
            _cachedMoons = allMoons;
            return allMoons;
        }
    }
}
