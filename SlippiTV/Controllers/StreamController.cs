using Microsoft.AspNetCore.Mvc;
using SlippiTV.Shared.SocketUtils;
using SlippiTV.Status;
using SlippiTV.Streams;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace SlippiTV.Controllers;

[ApiController]
public class StreamController : ControllerBase
{
    private readonly CancellationToken _shutdown;

    public StreamController(IHostApplicationLifetime lifetime)
    {
        _shutdown = lifetime.ApplicationStopping;
    }

    [HttpGet("stream/{user}")]
    public async Task Stream(string user, CancellationToken cancellation)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            try
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var requestClosing = new CancellationTokenSource();
                var anyCancel = CancellationTokenSource.CreateLinkedTokenSource(_shutdown, requestClosing.Token).Token;

                var stream = StreamManager.CreateOrUpdateStream(user);
                try
                {
                    await SocketUtils.ReceiveSocket(webSocket, x => stream.WriteData(x), anyCancel);
                }
                finally
                {
                    requestClosing.Cancel();
                }

                StreamManager.EndStream(user);
                await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "server is stopping", CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                        break;

                    default:
                        break;
                }
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
        ActiveStream? stream = StreamManager.GetStreamForUser(user);
        if (stream != null)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    var requestClosing = new CancellationTokenSource();
                    var anyCancel = CancellationTokenSource.CreateLinkedTokenSource(_shutdown, requestClosing.Token).Token;

                    var sendTask = Task.Run(async () =>
                    {
                        try
                        {
                            await SocketUtils.SendSocket(webSocket, stream.GetDataStream(), anyCancel);
                        }
                        finally
                        {
                            requestClosing.Cancel();
                        }
                    });

                    try
                    {
                        await SocketUtils.ReceiveSocket(webSocket, static x => { }, anyCancel);
                    }
                    finally
                    {
                        requestClosing.Cancel();
                    }

                    await sendTask;

                    await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server is stopping.", CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    switch (ex.WebSocketErrorCode)
                    {
                        case WebSocketError.ConnectionClosedPrematurely:
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
