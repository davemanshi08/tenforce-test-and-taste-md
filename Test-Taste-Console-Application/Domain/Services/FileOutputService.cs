using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services.Interfaces;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class FileOutputService : IFileOutputService
    {
        private const string Separator = ",";

        private readonly IPlanetService _planetService;
        private readonly IMoonService _moonService;

        public FileOutputService(IPlanetService planetService, IMoonService moonService)
        {
            _planetService = planetService;
            _moonService = moonService;
        }

        public void OutputAllPlanetsAndTheirAverageMoonTemperatureToFile()
        {
            //Will contain same data as the console output: only planets with at least one moon. unknown average will be written as an empty value
            var rows = _planetService.GetAllPlanets()
                .Where(planet => planet.HasMoons())
                .Select(planet => Row(
                    planet.Id,
                    planet.Moons.Count.ToString(CultureInfo.InvariantCulture),
                    planet.MoonsWithKnownTemperature.ToString(CultureInfo.InvariantCulture),
                    planet.AverageMoonTemperature?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty));

            WriteCsv(PathName.AllPlanetsAndTheirAverageMoonTemperatureFile,
                Row("PlanetId", "TotalMoons", "MoonsWithKnownTemperature", "AverageMoonTemperatureKelvin"), rows);
        }

        public void OutputAllMoonsAndTheirMassToFile()
        {
            var rows = _moonService.GetAllMoons()
                .Select(moon => Row(
                    moon.Id,
                    moon.MassValue.ToString(CultureInfo.InvariantCulture),
                    moon.MassExponent.ToString(CultureInfo.InvariantCulture)));

            WriteCsv(PathName.AllMoonsAndTheirMassFile, Row("MoonId", "MassValue", "MassExponent"), rows);
        }

        public void OutputAllPlanetsAndTheirMoonsToFile()
        {
            //contain one row per moon. A planet without moons will get one row with an empty moon id.
            var rows = new List<string>();
            foreach (var planet in _planetService.GetAllPlanets())
            {
                var semiMajorAxis = planet.SemiMajorAxis.ToString(CultureInfo.InvariantCulture);
                if (!planet.HasMoons())
                {
                    rows.Add(Row(planet.Id, semiMajorAxis, string.Empty));
                    continue;
                }

                rows.AddRange(planet.Moons.Select(moon => Row(planet.Id, semiMajorAxis, moon.Id)));
            }

            WriteCsv(PathName.AllPlanetsAndTheirMoonsFile, Row("PlanetId", "SemiMajorAxis", "MoonId"), rows);
        }

        private static void WriteCsv(string fileName, string header, IEnumerable<string> rows)
        {
            if (!Directory.Exists(PathName.PathToOutputFolder))
            {
                Directory.CreateDirectory(PathName.PathToOutputFolder);
                Console.WriteLine($"{OutputString.FolderCreated}{PathName.PathToOutputFolder}");
            }

            var filePath = Path.Combine(PathName.PathToOutputFolder, fileName);
            File.WriteAllLines(filePath, new[] { header }.Concat(rows));
            Console.WriteLine($"{OutputString.FileCreated}{filePath}");
        }

        //Will join the values to one CSV line. Values containing the separator or quotes are quoted.
        private static string Row(params string[] values)
        {
            return string.Join(Separator, values.Select(Escape));
        }

        private static string Escape(string value)
        {
            if (value == null) return string.Empty;
            return value.Contains(Separator) || value.Contains("\"")
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }
    }
}
