namespace Test_Taste_Console_Application.Constants
{
    public static class ExceptionMessage
    {
        public const string FileOutputOperationFailed = "The file output operation failed with the following error: ";

        public const string ScreenOutputOperationFailed =
            "The screen output operation failed with the following error: ";

        public const string ApiKeyMissing =
            "No API key found. Need to generate API Key, please generate api key first at https://api.le-systeme-solaire.net/generatekey.html and set the environment variable ";

        public const string ApiRequestFailedMessage = "The Solar System API request failed with status ";
        public const string CheckApiKeyMessage = "Check that a valid API key is set in the environment variable ";
        public const string ApiNotReachableMessage = "The Solar System API could not be reached: ";
        public const string ApiTimeoutMessage = "The Solar System API did not respond in time.";
        public const string ApiInvalidResponseMessage = "The Solar System API returned data in an unexpected format: ";
        public const string LoadingDataFailedMessage = "Loading data failed. No output is created. Reason: ";
    }
}
