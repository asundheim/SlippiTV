using System.Collections.Concurrent;

namespace SlippiTV.Status;

public static class StatusManager
{
    private static readonly ConcurrentDictionary<string, PlayerStatus> _players = new ConcurrentDictionary<string, PlayerStatus>();

    public static bool IsLive(string user)
    {
        return _players.TryGetValue(user, out PlayerStatus? status) && status.IsLive;
    }

    public static void GoLive(string user)
    {
        _players.AddOrUpdate(user,
            addValueFactory: (u) =>
            {
                PlayerStatus status = new PlayerStatus();
                status.GoLive();

                return status;
            },
            updateValueFactory: (u, old) =>
            {
                old.GoLive();
                return old;
            });
    }

    public static bool HasWatchers(string user)
    {
        return _players.TryGetValue(user, out PlayerStatus? status) && status.WatcherCount > 0;
    }
}
