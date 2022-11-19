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

        [HttpGet("{id}/start")]
        public string StartSnow(string id)
        {
            _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.StartSnow);
            return "snow started";
        }
    }
}
