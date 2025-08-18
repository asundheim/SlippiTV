using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace SlippiTV.Shared.SocketUtils;

public static class SocketUtils
{
    public static async Task ReceiveSocket(WebSocket socket, Action<byte[]> onData, CancellationToken cancellation)
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
                    onData(buffer.AsSpan().Slice(0, response.Count).ToArray());
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
