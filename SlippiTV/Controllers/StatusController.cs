using Microsoft.AspNetCore.Mvc;
using SlippiTV.Status;

namespace SlippiTV.Controllers;

[ApiController]
[Route("/status")]
public class StatusController : ControllerBase
{
    public StatusController()
    {

    }

    [HttpGet("/activity/{user}")]
    public IActionResult IsLive(string user)
    {
        return Ok(StatusManager.IsLive(user));
    }

    [HttpPost("/activity/{user}")]
    public IActionResult GoLive(string user)
    {
        StatusManager.GoLive(user);
        return Ok();
    }

    [HttpGet("/stream/{user}/currentFrame")]
    public IActionResult GetCurrentFrameOfStream(string user)
    {
        return Ok(0);
    }

    [HttpGet("/watchers/{user}")]
    public IActionResult HasWatchers(string user)
    {
        return Ok(StatusManager.HasWatchers(user));
    }
}
