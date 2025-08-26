using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlippiTV.Server.Streams;
using SlippiTV.Shared.Types;

namespace SlippiTV.Server.Controllers;

[ApiController]
public class StatusController : ControllerBase
{
    public StatusController()
    {
    }

    [HttpGet("status/activity/{user}")]
    public IActionResult GetUserStatusInfo(string user)
    {
        ActiveStream? stream = StreamManager.GetStreamForUser(user);
        if (stream is not null)
        {
            UserStatusInfo statusInfo = new UserStatusInfo();
            if (stream.IsActive)
            {
                statusInfo.LiveStatus = LiveStatus.Active;
                statusInfo.ActiveGameInfo = stream.ActiveGameInfo;
                statusInfo.ActiveViewerInfo = new ActiveViewerInfo() { ActiveViewerCount = stream.WatcherCount };
            }
            else
            {
                statusInfo.LiveStatus = LiveStatus.Idle;
                statusInfo.ActiveViewerInfo = new ActiveViewerInfo() { ActiveViewerCount = stream.WatcherCount };
            }

            return Ok(JsonConvert.SerializeObject(statusInfo));
        }
        else
        {
            return NotFound();
        }
    }
}
