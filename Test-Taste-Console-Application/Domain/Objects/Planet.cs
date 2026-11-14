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
        public float SemiMajorAxis { get; set; }
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

        public Planet(PlanetDto planetDto, IEnumerable<Moon> moons)
        {
            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
            Moons = new Collection<Moon>((moons ?? Enumerable.Empty<Moon>()).ToList());
        }

        public Boolean HasMoons()
        {
            return (Moons != null && Moons.Count > 0);
        }
    }
}
