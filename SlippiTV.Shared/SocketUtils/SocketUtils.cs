using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;

namespace SlippiTV.Shared.SocketUtils;

public static class SocketUtils
{
    public static async Task HandleSocket(WebSocket socket, Action<byte[]>? receiveData, BlockingCollection<byte[]>? sendData, CancellationToken cancellationToken)
    {
        try
        {
            var requestClosing = new CancellationTokenSource();
            var anyCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, requestClosing.Token).Token;

            var sendTask = Task.Run(async () =>
            {
                if (sendData is not null)
                {
                    try
                    {
                        await SendSocket(socket, sendData, anyCancel);
                    }
                    finally
                    {
                        requestClosing.Cancel();
                    }
                }
            });

            try
            {
                await ReceiveSocket(socket, receiveData, anyCancel);
            }
            finally
            {
                requestClosing.Cancel();
            }

            await sendTask;

            await socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server is stopping.", CancellationToken.None);
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

    public static async Task ReceiveSocket(WebSocket socket, Action<byte[]>? onData, CancellationToken cancellation)
    {
        const int maxMessageSize = 4096;
        var buffer = new byte[maxMessageSize];

        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
                if (response.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                else
                {
                    onData?.Invoke(buffer.AsSpan().Slice(0, response.Count).ToArray());
                }
            }
            catch (OperationCanceledException)
            {
                // Exit normally
            }
        }
    }

    public static async Task SendSocket(WebSocket socket, BlockingCollection<byte[]> source, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            if (!source.TryTake(out byte[]? data, 20, cancellation))
            {
                if (source.IsCompleted)
                {
                    break;
                }

                try
                {
                    await Task.Delay(100, cancellation);
                }
                catch (OperationCanceledException) 
                {
                    break;
                }
                
                continue;
            }

            try
            {
                await socket.SendAsync(data, WebSocketMessageType.Binary, endOfMessage: true, cancellation);
            }
            catch (OperationCanceledException)
            {
                // Exit
            }
        }
    }
}
