using Microsoft.AspNetCore.Mvc;
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
}
