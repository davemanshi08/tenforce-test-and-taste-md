namespace Test_Taste_Console_Application.Domain.Services.Interfaces
{
    ///<summary>
    /// An output service which will write data from the Solar System OpenData API<see href="https://api.le-systeme-solaire.net/"/> to CSV files on disk. 
    ///</summary>
    public interface IFileOutputService
    {
        void OutputAllPlanetsAndTheirAverageMoonTemperatureToFile();
        void OutputAllMoonsAndTheirMassToFile();
        void OutputAllPlanetsAndTheirMoonsToFile();
    }
}
