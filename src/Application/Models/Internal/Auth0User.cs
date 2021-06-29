namespace YA.UserWorker.Application.Models.Internal
{
    public class Auth0User
    {
#pragma warning disable IDE1006 // Naming Styles
        public string user_id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public AppMetadata app_metadata { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
