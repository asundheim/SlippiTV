using Microsoft.AspNetCore.Mvc;
using SlippiTV.Shared.Versions;

namespace SlippiTV.Server.Controllers;

public class UpdateController : Controller
{
    [HttpGet("/update/currentversion")]
    public IActionResult GetCurrentClientVersion()
    {
        // Return the version we were built with - if the client is out of sync, it should call GetUpdateScript to do an update
        return Ok(ClientVersion.SlippiClientVersion);
    }

    [HttpGet("/update/updatescript")]
    public IActionResult GetUpdateScript()
    {
        return Ok(UpdateScript.Powershell);
    }
}
