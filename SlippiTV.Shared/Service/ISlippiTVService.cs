using SlippiTV.Shared.Types;
using System.Net.WebSockets;

namespace SlippiTV.Shared.Service;

public interface ISlippiTVService : IDisposable
{
    public string SlippiTVServerHost { get; }
    Task<ClientWebSocket> Stream(string user);

    Task<ClientWebSocket> WatchStream(string user);

    Task<UserStatusInfo> GetStatus(string user);

    Task<string> GetCurrentClientVersion();

    Task<UpdateInfo> GetUpdateInfo();
}
