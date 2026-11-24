using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Xunit;

namespace Test_Taste_Console_Application.Tests
{
    ///<summary>
    /// Tests the CSV files written by the FileOutputService, using fake planet and moon services
    ///</summary>
    public class FileOutputServiceTests
    {
        private static Moon CreateMoon(string id, string planetId, float avgTemp = 0, float massValue = 0, float massExponent = 0) =>
            new Moon(new MoonDto
            {
                Id = id,
                AvgTemp = avgTemp,
                MassValue = massValue,
                MassExponent = massExponent,
                AroundPlanetRel = $"https://api.le-systeme-solaire.net/rest/bodies/{planetId}"
            });

        private static Planet CreatePlanet(string id, IEnumerable<Moon> allMoons, float planetAvgTemp = 0) =>
            new Planet(new PlanetDto { Id = id, SemiMajorAxis = 1000, AvgTemp = planetAvgTemp }, allMoons.Where(moon => moon.PlanetId == id));

        private static FileOutputService CreateService(List<Moon> moons, params string[] planetIds) =>
            new FileOutputService(
                new FakePlanetService(planetIds.Select(id => CreatePlanet(id, moons)).ToList()),
                new FakeMoonService(moons));

        private static FileOutputService CreateService(List<Moon> moons, List<Planet> planets) =>
            new FileOutputService(new FakePlanetService(planets), new FakeMoonService(moons));

        private static string[] ReadCsv(string fileName) =>
            File.ReadAllLines(Path.Combine(PathName.PathToOutputFolder, fileName));

        [Fact]
        public void AverageMoonTemperatureFile_WritesKnownAverage_EmptyWhenUnknown_SkipsPlanetsWithoutMoons()
        {
            var moons = new List<Moon>
            {
                CreateMoon("phobos", "mars", avgTemp: 233),
                CreateMoon("deimos", "mars", avgTemp: 0),
                CreateMoon("io", "jupiter", avgTemp: 0)
            };

            CreateService(moons, "mars", "jupiter", "venus").OutputAllPlanetsAndTheirAverageMoonTemperatureToFile();
            var lines = ReadCsv(PathName.AllPlanetsAndTheirAverageMoonTemperatureFile);

            Assert.Equal(new[]
            {
                "PlanetId,TotalMoons,AverageMoonTemperatureKelvin",
                "mars,2,233",
                "jupiter,1,"
            }, lines);
        }

        [Fact]
        public void AverageMoonTemperatureFile_NoMoonData_UsesPlanetTemperatureAsEstimate()
        {
            var moons = new List<Moon> { CreateMoon("io", "jupiter", avgTemp: 0) };
            var planets = new List<Planet> { CreatePlanet("jupiter", moons, planetAvgTemp: 165) };

            CreateService(moons, planets).OutputAllPlanetsAndTheirAverageMoonTemperatureToFile();
            var lines = ReadCsv(PathName.AllPlanetsAndTheirAverageMoonTemperatureFile);

            Assert.Equal(new[]
            {
                "PlanetId,TotalMoons,AverageMoonTemperatureKelvin",
                "jupiter,1,165"
            }, lines);
        }

        [Fact]
        public void MoonsAndTheirMassFile_UsesInvariantCulture_EvenWithCommaDecimalCulture()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("nl-BE");
                var moons = new List<Moon> { CreateMoon("lune", "terre", massValue: 7.346f, massExponent: 22) };

                CreateService(moons, "terre").OutputAllMoonsAndTheirMassToFile();
                var lines = ReadCsv(PathName.AllMoonsAndTheirMassFile);

                Assert.Equal(new[] { "MoonId,MassValue,MassExponent", "lune,7.346,22" }, lines);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void PlanetsAndTheirMoonsFile_OneRowPerMoon_PlanetWithoutMoonsHasEmptyMoonId()
        {
            var moons = new List<Moon> { CreateMoon("phobos", "mars"), CreateMoon("deimos", "mars") };

            CreateService(moons, "mars", "venus").OutputAllPlanetsAndTheirMoonsToFile();
            var lines = ReadCsv(PathName.AllPlanetsAndTheirMoonsFile);

            Assert.Equal(new[]
            {
                "PlanetId,SemiMajorAxis,MoonId",
                "mars,1000,phobos",
                "mars,1000,deimos",
                "venus,1000,"
            }, lines);
        }

        [Fact]
        public void MoonsAndTheirMassFile_ValueWithCommaOrQuote_IsQuotedAndEscaped()
        {
            var moons = new List<Moon>
            {
                CreateMoon("moon,with comma", "terre", massValue: 1, massExponent: 2),
                CreateMoon("moon \"quoted\"", "terre", massValue: 1, massExponent: 2)
            };

            CreateService(moons, "terre").OutputAllMoonsAndTheirMassToFile();
            var lines = ReadCsv(PathName.AllMoonsAndTheirMassFile);

            Assert.Equal("\"moon,with comma\",1,2", lines[1]);
            Assert.Equal("\"moon \"\"quoted\"\"\",1,2", lines[2]);
        }

        private class FakePlanetService : IPlanetService
        {
            private readonly IEnumerable<Planet> _planets;
            public FakePlanetService(IEnumerable<Planet> planets) => _planets = planets;
            public IEnumerable<Planet> GetAllPlanets() => _planets;
        }

        private class FakeMoonService : IMoonService
        {
            private readonly IEnumerable<Moon> _moons;
            public FakeMoonService(IEnumerable<Moon> moons) => _moons = moons;
            public IEnumerable<Moon> GetAllMoons() => _moons;
        }
    }
}
