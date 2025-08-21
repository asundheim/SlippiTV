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
            BaseAddress = new Uri($"https://{baseAddress}"),
        };
    }

    public async Task<LiveStatus> GetStatus(string user)
    {
        var result = await _client.GetAsync($"/status/activity/{SanitizeConnectCode(user)}", CancellationToken.None);
        return result.StatusCode switch
        {
            HttpStatusCode.OK => LiveStatus.Active,
            HttpStatusCode.NoContent => LiveStatus.Idle,
            _ => LiveStatus.Offline
        };
    }

    public async Task<ClientWebSocket> Stream(string user)
    {
        ClientWebSocket clientSocket = new ClientWebSocket();
        clientSocket.Options.KeepAliveInterval = Timeout.InfiniteTimeSpan;
        await clientSocket.ConnectAsync(new Uri($"wss://{_baseAddress}/stream/{SanitizeConnectCode(user)}"), CancellationToken.None);

        return clientSocket;
    }

    public async Task<ClientWebSocket> WatchStream(string user)
    {
        ClientWebSocket clientSocket = new ClientWebSocket();
        clientSocket.Options.KeepAliveInterval = Timeout.InfiniteTimeSpan;
        await clientSocket.ConnectAsync(new Uri($"wss://{_baseAddress}/stream/{SanitizeConnectCode(user)}/watch"), CancellationToken.None);

        return clientSocket;
    }

    private static string SanitizeConnectCode(string code) => code.Replace("#", "-");

    public void Dispose()
    {
        _client.Dispose();
    }
}
