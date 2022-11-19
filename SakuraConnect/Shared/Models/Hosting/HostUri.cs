
namespace SakuraConnect.Shared.Models.Hosting
{
    public static class HostUri
    {
        /// <summary>
        /// Gets the address to the backend server
        /// </summary>
#if DEBUG
        public const string Backend = "https://localhost:7206";
#else
        public const string Backend = "https://sakuraconnect.azurewebsites.net";
#endif
    }
}
