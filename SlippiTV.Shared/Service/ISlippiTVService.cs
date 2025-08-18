using Refit;
using System.Net.WebSockets;

namespace SlippiTV.Shared.Service;

public interface ISlippiTVService : IDisposable
{
    Task<ClientWebSocket> Stream(string user);

    Task<ClientWebSocket> WatchStream(string user);

    Task<bool> IsLive(string user);
}
