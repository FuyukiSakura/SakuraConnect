using Microsoft.AspNetCore.SignalR;

namespace SakuraConnect.Server.Hubs.Overlays.Effects
{
    public class SnowfallHub : Hub
    {
        /// <summary>
        /// Adds leader board subscribers page to the group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task Subscribe(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) return; // No room id given

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
