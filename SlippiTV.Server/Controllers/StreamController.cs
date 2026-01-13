using Microsoft.AspNetCore.Mvc;
using SlippiTV.Server.Streams;
using SlippiTV.Shared.SocketUtils;

namespace SlippiTV.Server.Controllers;

[ApiController]
public class StreamController : ControllerBase
{
    private readonly CancellationToken _shutdown;
    private readonly StreamManager _streamManager;

    public StreamController(IHostApplicationLifetime lifetime, StreamManager streamManager)
    {
        _shutdown = lifetime.ApplicationStopping;
        _streamManager = streamManager;
    }

    [HttpGet("stream/{user}")]
    public async Task Stream(string user, CancellationToken cancellation)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            try
            {
                ActiveStream stream = _streamManager.CreateOrUpdateStream(user);

                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await SocketUtils.HandleSocket(webSocket, x => stream.WriteData(x), null, _shutdown);
            }
            catch { }
            finally
            {
                _streamManager.EndStream(user);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    [HttpGet("stream/{user}/watch")]
    public async Task WatchStream(string user, CancellationToken cancellation)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            ActiveStream? stream = _streamManager.GetStreamForUser(user);
            if (stream != null)
            {
                try
                {
                    stream.AddWatcher();
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    await SocketUtils.HandleSocket(webSocket, null, stream.GetDataStream(), _shutdown);
                }
                catch { }
                finally
                {
                    stream.RemoveWatcher();
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
