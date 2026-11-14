using System.Linq;
using Test_Taste_Console_Application.Domain.DataTransferObjects;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Moon
    {
        public string Id { get; set; }
        public float MassValue { get; set; }
        public float MassExponent { get; set; }

        //to store mean temperature in Kelvin, or null when unknown.
        public float? AverageTemperatureKelvin { get; set; }

        //to store Id of the planet this moon orbits 
        public string PlanetId { get; set; }

        public Moon(MoonDto moonDto)
        {
            Id = moonDto.Id;
            MassValue = moonDto.MassValue;
            MassExponent = moonDto.MassExponent;
            AverageTemperatureKelvin = moonDto.AvgTemp > 0 ? moonDto.AvgTemp : (float?)null;

            //The planet id is the last segment of the planet's URL.
            PlanetId = string.IsNullOrWhiteSpace(moonDto.AroundPlanetRel)
                ? null
                : moonDto.AroundPlanetRel.TrimEnd('/').Split('/').Last();
        }
    }
}
