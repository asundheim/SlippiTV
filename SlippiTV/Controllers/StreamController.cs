using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SlippiTV.Streams;
using SlippiTV.Utilities;

namespace SlippiTV.Controllers;

[ApiController]
[Route("/stream")]
public class StreamController : ControllerBase
{
    public StreamController()
    {

    }

    [HttpPost("/{user}")]
    public async Task<IActionResult> Stream(string user)
    {
        Stream fileStream = HttpContext.Request.Body;
        await StreamManager.SetStreamAndWait(user, fileStream);

        return Ok();
    }

    [HttpGet("/{user}")]
    public IActionResult WatchStream(string user)
    {
        Stream? fileStream = StreamManager.GetStreamForUser(user);
        if (fileStream is not null)
        {
            return new FileStreamResult(fileStream, contentType: "application/octet-stream");
        }
        else
        {
            return NotFound();
        }
    }
}
