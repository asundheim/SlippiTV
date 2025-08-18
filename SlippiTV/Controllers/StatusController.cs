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
    public IActionResult IsLive(string user)
    {
        return (StreamManager.GetStreamForUser(user)?.IsActive ?? false) ? Ok() : NotFound();
    }
}
