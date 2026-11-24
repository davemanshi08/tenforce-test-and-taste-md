using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Test_Taste_Console_Application.Domain.DataTransferObjects;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Planet
    {
        public string Id { get; set; }
        public double SemiMajorAxis { get; set; }
        public ICollection<Moon> Moons { get; set; }

        //to store number of moons that have a known temperature
        public int MoonsWithKnownTemperature => Moons.Count(moon => moon.AverageTemperatureKelvin.HasValue);

        //to store average temperature (K) of this planet's moons, ignoring moons with an unknown temperature.
        public float? AverageMoonTemperature
        {
            get
            {
                var knownTemperatures = Moons
                    .Where(moon => moon.AverageTemperatureKelvin.HasValue)
                    .Select(moon => moon.AverageTemperatureKelvin.Value)
                    .ToList();

                return knownTemperatures.Count > 0 ? knownTemperatures.Average() : (float?)null;
            }
        }

        //to store the planet's own mean temperature (K) from the API, or null when unknown.
        public float? AverageTemperatureKelvin { get; set; }

        //The API currently has no temperature for any moon (avgTemp = 0), so when none of the moons has data.
        public float? AverageMoonTemperatureKelvin => AverageMoonTemperature ?? AverageTemperatureKelvin;

        //True when AverageMoonTemperatureKelvin comes from the planet and not from the moons themselves.
        public bool IsMoonTemperatureEstimated => !AverageMoonTemperature.HasValue && AverageTemperatureKelvin.HasValue;

        public Planet(PlanetDto planetDto, IEnumerable<Moon> moons)
        {
            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
            AverageTemperatureKelvin = planetDto.AvgTemp > 0 ? planetDto.AvgTemp : (float?)null;
            Moons = new Collection<Moon>((moons ?? Enumerable.Empty<Moon>()).ToList());
        }

        public Boolean HasMoons()
        {
            return (Moons != null && Moons.Count > 0);
        }
    }
}
