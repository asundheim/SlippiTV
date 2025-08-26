using System.Collections.Concurrent;

namespace SlippiTV.Server.Streams;

public static class StreamManager
{
    private static readonly ConcurrentDictionary<string, ActiveStream> _streams = new ConcurrentDictionary<string, ActiveStream>();

    public static ActiveStream CreateOrUpdateStream(string user)
    {
        ActiveStream activeStream = new ActiveStream(user);
        _streams.AddOrUpdate(user, activeStream, (_, old) =>
        {
            old.Dispose();
            return activeStream;
        });

        return activeStream;
    }

    public static bool EndStream(string user)
    {
        var activeStream = GetStreamForUser(user);
        if (activeStream is not null)
        {
            activeStream.Dispose();
            return _streams.TryRemove(user, out _);
        }

        return false;
    }

    public static ActiveStream? GetStreamForUser(string user)
    {
        if (_streams.TryGetValue(user, out ActiveStream? stream))
        {
            return stream;
        }
        else
        {
            return null;
        }
    }
}
