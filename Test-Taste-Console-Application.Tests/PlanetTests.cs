using System.Linq;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Xunit;

namespace Test_Taste_Console_Application.Tests
{
    public class PlanetTests
    {
        private static Moon CreateMoon(string id, float avgTemp) =>
            new Moon(new MoonDto { Id = id, AvgTemp = avgTemp, AroundPlanetRel = "https://api.le-systeme-solaire.net/rest/bodies/mars" });

        private static Planet CreatePlanet(params Moon[] moons) =>
            new Planet(new PlanetDto { Id = "mars", SemiMajorAxis = 227939200 }, moons);

        [Fact]
        public void AverageMoonTemperature_AllTemperaturesKnown_ReturnsAverage()
        {
            var planet = CreatePlanet(CreateMoon("phobos", 250), CreateMoon("deimos", 100));

            Assert.Equal(175f, planet.AverageMoonTemperature);
            Assert.Equal(2, planet.MoonsWithKnownTemperature);
        }

        [Fact]
        public void AverageMoonTemperature_SomeTemperaturesUnknown_IgnoresUnknown()
        {
            //0 means "unknown" in the API; it must not pull the average down.
            var planet = CreatePlanet(CreateMoon("phobos", 250), CreateMoon("deimos", 0));

            Assert.Equal(250f, planet.AverageMoonTemperature);
            Assert.Equal(1, planet.MoonsWithKnownTemperature);
        }

        [Fact]
        public void AverageMoonTemperature_AllTemperaturesUnknown_ReturnsNull()
        {
            var planet = CreatePlanet(CreateMoon("phobos", 0), CreateMoon("deimos", 0));

            Assert.Null(planet.AverageMoonTemperature);
            Assert.Equal(0, planet.MoonsWithKnownTemperature);
        }

        [Fact]
        public void AverageMoonTemperature_NoMoons_ReturnsNull()
        {
            var planet = CreatePlanet();

            Assert.Null(planet.AverageMoonTemperature);
            Assert.False(planet.HasMoons());
        }

        [Fact]
        public void Constructor_NullMoons_CreatesPlanetWithoutMoons()
        {
            var planet = new Planet(new PlanetDto { Id = "venus" }, null);

            Assert.NotNull(planet.Moons);
            Assert.False(planet.HasMoons());
        }
    }
}
