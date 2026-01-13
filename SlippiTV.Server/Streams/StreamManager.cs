using System.Collections.Concurrent;

namespace SlippiTV.Server.Streams;

public class StreamManager
{
    private readonly ConcurrentDictionary<string, ActiveStream> _streams = new ConcurrentDictionary<string, ActiveStream>();

    public ActiveStream CreateOrUpdateStream(string user)
    {
        ActiveStream activeStream = new ActiveStream(user);
        _streams.AddOrUpdate(user, activeStream, (_, old) =>
        {
            old.Dispose();
            return activeStream;
        });

        return activeStream;
    }

    public bool EndStream(string user)
    {
        var activeStream = GetStreamForUser(user);
        if (activeStream is not null)
        {
            activeStream.Dispose();
            return _streams.TryRemove(user, out _);
        }

        return false;
    }

    public ActiveStream? GetStreamForUser(string user)
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
