using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SakuraConnect.Server.Hubs.Overlays.Effects;
using SakuraConnect.Shared.Models.Hubs.Overlays.Effects;

namespace SakuraConnect.Server.Controllers.Overlays.Effects
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/overlays/effects/[controller]")]
    public class SnowfallController : ControllerBase
    {
        readonly IHubContext<SnowfallHub> _hubContext;

        /// <summary>
        /// Creates a new instance of <see cref="SnowfallController"/>
        /// </summary>
        public SnowfallController(IHubContext<SnowfallHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Notifies all client in the group to start the snow
        /// </summary>
        /// <param name="id">id of the group of subscribers</param>
        /// <returns></returns>
        [HttpGet("{id}/start")]
        public string StartSnow(string id)
        {
            _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.StartSnow);
            return "snow started";
        }

        /// <summary>
        /// Notifies all client in the group to change how the snows are rendered
        /// </summary>
        /// <param name="id">id of the group of subscribers</param>
        /// <param name="icon">The icon of the snow</param>
        /// <param name="snowFlakes">Number of snow flakes to render</param>
        /// <returns></returns>
        [HttpGet("{id}/change")]
        public string ChangeIcon(string id, string? icon, int? snowFlakes)
        {
            if (icon != null)
            {
                _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.ChangeIcon, icon);
            }

            if (snowFlakes != null)
            {
                _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.ChangeNumber, snowFlakes);
            }
            return "snow changed";
        }
    }
}
