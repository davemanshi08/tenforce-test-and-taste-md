using System;

namespace Test_Taste_Console_Application.Domain.Exceptions
{
    ///<summary>
    /// This project will throw this exception when data can't be loaded from the Solar System OpenData API
    /// Errors such as (missing/invalid API key, error status code, network problem, timeout or unexpected response).
    ///</summary>
    public class SolarSystemApiException : Exception
    {
        public SolarSystemApiException(string message) : base(message)
        {
        }

        public SolarSystemApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
