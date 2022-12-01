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
        /// Notifies all client in the group to start the snow
        /// </summary>
        /// <param name="id">id of the group of subscribers</param>
        /// <returns></returns>
        [HttpGet("{id}/stop")]
        public string StopSnow(string id)
        {
            _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.StopSnow);
            return "snow stopped";
        }

        /// <summary>
        /// Notifies all client in the group to change how the snows are rendered
        /// </summary>
        /// <param name="id">id of the group of subscribers</param>
        /// <param name="icon">The icon of the snow</param>
        /// <param name="snowFlakes">Number of snow flakes to render</param>
        /// <returns></returns>
        [HttpGet("{id}/update")]
        public string UpdateSnow(string id, string? icon, int? snowFlakes, float? zoom)
        {
            if (icon != null)
            {
                _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.UpdateIcon, icon);
            }

            if (snowFlakes != null)
            {
                _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.UpdateNumber, snowFlakes);
            }

            if (zoom != null)
            {
                _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.UpdateZoom, zoom);
            }
            return "snow changed";
        }

        /// <summary>
        /// Notifies all client in the group to add/subtract number of the snowflakes
        /// </summary>
        /// <param name="id">id of the group of subscribers</param>
        /// <param name="count">number of snowflakes to add, can be negative</param>
        /// <returns></returns>
        [HttpGet("{id}/add")]
        public string AddSnow(string id, int count)
        {
            _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.AddSnow, count);
            return "snow added";
        }

        /// <summary>
        /// Notifies all client in the group to zoom in/out of the snow
        /// This makes snowflakes bigger or smaller
        /// </summary>
        /// <param name="id">id of the group of subscribers</param>
        /// <param name="ratio">ratio of zoom to increase, can be negative</param>
        /// <returns></returns>
        [HttpGet("{id}/zoom")]
        public string Zoom(string id, float ratio)
        {
            _hubContext.Clients.Group(id).SendAsync(SnowfallHubMessage.Zoom, ratio);
            return "snow zoomed";
        }
    }
}
