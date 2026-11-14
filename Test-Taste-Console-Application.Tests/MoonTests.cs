using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Xunit;

namespace Test_Taste_Console_Application.Tests
{
    public class MoonTests
    {
        [Fact]
        public void Constructor_TemperatureZero_IsMappedToUnknown()
        {
            var moon = new Moon(new MoonDto { Id = "io", AvgTemp = 0 });

            Assert.Null(moon.AverageTemperatureKelvin);
        }

        [Fact]
        public void Constructor_TemperatureKnown_IsKept()
        {
            var moon = new Moon(new MoonDto { Id = "lune", AvgTemp = 250 });

            Assert.Equal(250f, moon.AverageTemperatureKelvin);
        }

        [Theory]
        [InlineData("https://api.le-systeme-solaire.net/rest/bodies/terre", "terre")]
        [InlineData("https://api.le-systeme-solaire.net/rest/bodies/terre/", "terre")]
        [InlineData(null, null)]
        [InlineData("", null)]
        public void Constructor_AroundPlanetRel_ParsesPlanetId(string rel, string expectedPlanetId)
        {
            var moon = new Moon(new MoonDto { Id = "lune", AroundPlanetRel = rel });

            Assert.Equal(expectedPlanetId, moon.PlanetId);
        }
    }
}
