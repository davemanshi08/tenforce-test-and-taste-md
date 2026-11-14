using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application
{
    class Program
    {
        //Exit code 0 = success, 1 = something failed. Useful when the application runs in a script or scheduler.
        private const int ExitCodeSuccess = 0;
        private const int ExitCodeFailure = 1;

        static int Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            //The ConfigureServices function configures the services.
            ConfigureServices(serviceCollection);
            
            //The RunServiceOperations function executes the code that can create the outputs.
            return RunServiceOperations(serviceCollection);
        }

        private static int RunServiceOperations(IServiceCollection serviceCollection)
        {
            using var serviceProvider = serviceCollection.BuildServiceProvider();

            //The data is loaded once, up front. If that fails (missing API key, API down, ...)
            //the user gets one clear error message instead of empty outputs.
            try
            {
                serviceProvider.GetRequiredService<IPlanetService>().GetAllPlanets();
            }
            catch (Exception exception)
            {
                Logger.Instance.Error($"{ExceptionMessage.LoadingDataFailedMessage}{exception.Message}");
                return ExitCodeFailure;
            }

            var screenOutputService = serviceProvider.GetRequiredService<IOutputService>();
            var fileOutputService = serviceProvider.GetRequiredService<IFileOutputService>();

            //Each output runs on its own, so one failing output doesn't stop the others.
            var succeeded = true;
            succeeded &= RunOutput(OutputString.WritingAverageMoonTemperature,
                screenOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToConsole, ExceptionMessage.ScreenOutputOperationFailed);
            succeeded &= RunOutput(OutputString.WritingMoonsAndTheirMass,
                screenOutputService.OutputAllMoonsAndTheirMassToConsole, ExceptionMessage.ScreenOutputOperationFailed);
            succeeded &= RunOutput(OutputString.WritingPlanetsAndTheirMoons,
                screenOutputService.OutputAllPlanetsAndTheirMoonsToConsole, ExceptionMessage.ScreenOutputOperationFailed);

            Console.WriteLine(OutputString.WritingFiles);
            succeeded &= RunOutput(null,
                fileOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToFile, ExceptionMessage.FileOutputOperationFailed);
            succeeded &= RunOutput(null,
                fileOutputService.OutputAllMoonsAndTheirMassToFile, ExceptionMessage.FileOutputOperationFailed);
            succeeded &= RunOutput(null,
                fileOutputService.OutputAllPlanetsAndTheirMoonsToFile, ExceptionMessage.FileOutputOperationFailed);

            Console.WriteLine(succeeded ? OutputString.Finished : OutputString.FinishedWithErrors);
            return succeeded ? ExitCodeSuccess : ExitCodeFailure;
        }

        //Runs one output and reports a failure without stopping the application.
        private static bool RunOutput(string progressMessage, Action output, string failureMessage)
        {
            if (progressMessage != null) Console.WriteLine(progressMessage);

            try
            {
                output();
                return true;
            }
            catch (Exception exception)
            {
                //The users and developers can see the thrown exceptions.
                Logger.Instance.Error($"{failureMessage}{exception.Message}");
                System.Diagnostics.Debug.WriteLine($"{failureMessage}{exception}");
                return false;
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            //The function configures all the services.
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()),
                new FileInfo(ConfigurationFileName.Logger));
            serviceCollection.AddHttpClient<HttpClientService>();
            serviceCollection.AddSingleton<IPlanetService, PlanetService>();
            serviceCollection.AddSingleton<IOutputService, ScreenOutputService>();
            serviceCollection.AddSingleton<IFileOutputService, FileOutputService>();
            serviceCollection.AddSingleton<IMoonService, MoonService>();
        }
    }
}
