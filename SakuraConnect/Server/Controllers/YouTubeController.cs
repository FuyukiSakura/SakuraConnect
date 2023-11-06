using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SakuraConnect.Server.Controllers
{
    /// <summary>
    /// Helps with YouTube API
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class YouTubeController : ControllerBase
    {
        readonly ILogger<YouTubeController> _logger;

        public YouTubeController(ILogger<YouTubeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets latest live stream of a YouTube Channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        [HttpGet("LiveStream")]
        public string GetLiveStreamId(string channelId)
        {
            var apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");
            var url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&channelId={channelId}&eventType=live&type=video&key={apiKey}";
            var json = new WebClient().DownloadString(url);
            dynamic data = JsonConvert.DeserializeObject(json);
            bool hasLiveStream = data.items.Count > 0;
            return hasLiveStream ? data.items[0].id.videoId : "NOT_FOUND";
        }
    }
}