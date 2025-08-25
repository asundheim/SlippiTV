using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlippiTV.Streams;

namespace SlippiTV.Controllers;

[ApiController]
public class StatusController : ControllerBase
{
    public StatusController()
    {
    }

    [HttpGet("status/activity/{user}")]
    public IActionResult LiveStatus(string user)
    {
        ActiveStream? stream = StreamManager.GetStreamForUser(user);
        if (stream is not null)
        {
            return stream.IsActive ? Ok() : NoContent();
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("status/activity/{user}/game")]
    public IActionResult QueryGame(string user)
    {
        ActiveStream? stream = StreamManager.GetStreamForUser(user);
        if (stream is not null && stream.IsActive && stream.ActiveGameInfo is not null)
        {
            return Ok(JsonConvert.SerializeObject(stream.ActiveGameInfo));
        }
        else
        {
            return NotFound();
        }
    }
}
