namespace Test_Taste_Console_Application.Domain.DataTransferObjects
{
    public class PlanetDto
    {
        public string Id { get; set; }
        //updated datatype to double instead of float as the values are in the billions of km and float loses precision.
        public double SemiMajorAxis { get; set; }

        //Mean temperature of the planet in Kelvin (API field "avgTemp"). The API returns 0 when it is unknown.
        public float AvgTemp { get; set; }
    }
}