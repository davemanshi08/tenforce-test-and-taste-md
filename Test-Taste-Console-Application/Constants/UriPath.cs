namespace Test_Taste_Console_Application.Constants
{
    public static class UriPath
    {
        public const string BaseUri = "https://api.le-systeme-solaire.net";
        private const string BodiesUri = "/rest/bodies";

        //To load only planet fields which required. Loading moons details separately instead of calling the API once per moon.
        public const string GetAllPlanetsQueryParameters =
            BodiesUri + "?data=id,semimajorAxis&filter[]=isPlanet,neq,false";

        //All moons details in ONE request.
        public const string GetAllMoonsQueryParameters = BodiesUri + "?filter[]=bodyType,eq,Moon";
    }
}
