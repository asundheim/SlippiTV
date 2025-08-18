using System.Net;
using System.Net.WebSockets;

namespace SlippiTV.Shared.Service;

internal class SlippiTVService : ISlippiTVService
{
    private string _baseAddress;
    private readonly HttpClient _client;

    public SlippiTVService(string baseAddress)
    {
        _baseAddress = baseAddress;
        _client = new HttpClient()
        {
            BaseAddress = new Uri($"http://{baseAddress}"),
        };
    }

    public async Task<bool> IsLive(string user)
    {
        string sanitized = user.Replace("#", string.Empty);
        var result = await _client.GetAsync($"/status/activity/{SanitizeConnectCode(user)}", CancellationToken.None);
        return result.StatusCode switch
        {
            HttpStatusCode.OK => true,
            _ => false
        };
    }

    public async Task<ClientWebSocket> Stream(string user)
    {
        ClientWebSocket clientSocket = new ClientWebSocket();
        clientSocket.Options.KeepAliveInterval = Timeout.InfiniteTimeSpan;
        await clientSocket.ConnectAsync(new Uri($"ws://{_baseAddress}/stream/{SanitizeConnectCode(user)}"), CancellationToken.None);

        return clientSocket;
    }

    public async Task<ClientWebSocket> WatchStream(string user)
    {
        ClientWebSocket clientSocket = new ClientWebSocket();
        clientSocket.Options.KeepAliveInterval = Timeout.InfiniteTimeSpan;
        await clientSocket.ConnectAsync(new Uri($"ws://{_baseAddress}/stream/{SanitizeConnectCode(user)}/watch"), CancellationToken.None);

        return clientSocket;
    }

    private static string SanitizeConnectCode(string code) => code.Replace("#", "-");

    public void Dispose()
    {
        _client.Dispose();
    }
}
