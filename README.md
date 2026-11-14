# TenForce Test and Taste – Solar System Console Application

This console application reads data from the Solar System OpenData API (https://api.le-systeme-solaire.net) and shows planets and moons in the console. It also saves the same data to CSV files.

As part of this exercise I added a new output: a list of all planets that have at least one moon, with the average temperature of those moons.

## How to run

1. The API now requires an API key which was not the case when the exercise was written. You can get a free key here: https://api.le-systeme-solaire.net/generatekey.html
2. Save the key as an environment variable with following provided code, so it is not stored in the code:
   `setx SOLAR_SYSTEM_API_KEY "your-key"`
   After this, please restart Visual Studio or the terminal.
3. Run the `Test-Taste-Console-Application` project.
The application shows its progress ("Loading planets...", "Writing data..."), prints three tables, and writes three CSV files to the `FileOutput` folder next to the exe (`bin/Debug/net5.0/FileOutput`). If something goes wrong (no key, wrong key, no internet), a clear error message is shown and the application ends with exit code 1. When everything works the exit code is 0. To run the unit tests: `dotnet test`. The tests do not call the real API, so no key or internet is needed.

## About the temperature result

The new table shows "N/A" for every planet. This is because the API currently has no temperature for any moon: all 479 moons return `avgTemp = 0`. I checked this directly on a few moons (Lune, Titan, Io) to make sure it was not a problem in my code. A temperature of 0 Kelvin is not realistic, so I treat 0 as "unknown" and leave it out of the average. If I had included it, every planet would show 0 K, which looks like a valid result but is wrong. Next to the average I also show how many moons actually had a temperature, so the user can see how reliable the number is. The calculation itself is covered by unit tests with real values, so it will show correct averages as soon as the API has data.

## Problems I found in the starter project

- The project did not build because of a wrongly written string in `Program.cs`.
- Every API call failed with "401 Unauthorized" because the API now requires a key.
- The app showed "No planets found" when the API call failed, so the real problem was hidden. It also ended with exit code 0, as if everything was fine.
- The average was calculated for gravity, and it was hard-coded to 0. There was no temperature in the code yet.
- The app called the API once for every moon (around 400 calls), and it loaded all planets twice. One run took several minutes.
- The query used for the moon list missed all moons of Mars, Jupiter and Neptune (336 of 479 moons were returned).
- The planet query asked for `semiMajorAxis`, but the field in the API is `semimajorAxis`.
- Saving to a file was mentioned in the exercise and prepared in the code, but not implemented.
- Planets without moons were shown in the moon related output.
- One error stopped all remaining output.

## Changes and why I made them

### Data model
- A moon now has a temperature. When the API returns 0, I store it as "no value" (null) instead of 0, so it can never be counted in the average by mistake.
- A planet now calculates the average temperature of its moons, using only the moons that have a known temperature. If none of them has one, the result is "no value" and the screen shows "N/A".
- A moon also knows which planet it belongs to. This makes it easy to group moons per planet.
- The old gravity property was replaced, because it always returned 0 and was not part of the task.
- The semi-major axis is now stored as double instead of float. The values are in the billions of km, and `float` lost precision (Earth showed 149598020 instead of 149598023). On screen it is now shown as 2,870,658,186 instead of 2.8706583E+09.

### API calls
- I replaced the 800 calls with 2: one call for all planets and one for all moons. The moons are then matched to their planet in code. The app now runs in a few seconds instead of a few minutes, and it puts much less load on a free public API.
- I changed the moon query to filter on bodyType = Moon, which returns all 479 moons.
- The loaded data is kept in memory, so the three outputs do not call the API again.
- All error handling for API calls is in one place (HttpClientService). It handles wrong status codes, network errors and timeouts, and gives a readable message. For 401/403 it also tells the user to check the API key.

### Output
- New FileOutputService that writes three CSV files.
- Numbers in the CSV always use a dot as decimal separator. Otherwise, on a computer with Belgian or Dutch settings, a value like 7,346 would break the CSV columns.
- Each output runs on its own, so if one fails (for example a file is open in Excel), the others still run.

## Testing

I added a test project with 19 unit tests. They cover:
- the average temperature (all known, some unknown, all unknown, no moons)
- moons being matched to the correct planet, using only 2 API calls (with a fake API, so no internet is needed)
- error cases, like a wrong key or invalid data from the API
- the CSV files (empty value for unknown temperature, correct number format, special characters)

I also tested manually: a normal run, no API key, a wrong API key, no internet, and a CSV file that was locked because it was open in Excel.

## What I would do with more time

- Upgrade from .NET 5 (no longer supported) to .NET 8.
- Treat a mass of 0 as unknown too, the same way as temperature (about 230 moons have no mass).
- Make the API calls fully asynchronous.
- Small cleanups: a duplicate .sln file inside the project folder, a typo in an error message in JsonPathConverter, and a message constant that is no longer used.