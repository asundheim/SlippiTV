using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlippiTV.Shared.Types;
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

    [HttpGet("/update/updateinfo")]
    public IActionResult GetUpdateInfo()
    {
        return Ok(JsonConvert.SerializeObject(new UpdateInfo()
        {
            UpdateScript = UpdateScript.Powershell,
            UpdateLink = UpdateScript.UpdateDownloadLink,
            UpdateFileName = UpdateScript.UpdateZipName
        }));
    }
}
