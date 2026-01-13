using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlippiTV.Server.Streams;
using SlippiTV.Shared.Types;

namespace SlippiTV.Server.Controllers;

[ApiController]
public class StatusController : ControllerBase
{
    private readonly StreamManager _streamManager;

    public StatusController(StreamManager streamManager)
    {
        _streamManager = streamManager;
    }

    [HttpGet("status/activity/{user}")]
    public IActionResult GetUserStatusInfo_Legacy(string user)
    {
        ActiveStream? stream = _streamManager.GetStreamForUser(user);
        if (stream is not null)
        {
            return stream.IsActive ? Ok() : NoContent();
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("status/activity/{user}/all")]
    public async Task GetUserStatusInfo(string user)
    {
        ActiveStream? stream = _streamManager.GetStreamForUser(user);
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

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(statusInfo));
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
