using System.Collections.Concurrent;
using System.Net.Sockets;

namespace SlippiTV.Streams;

public static class StreamManager
{
    private static readonly ConcurrentDictionary<string, FileStream> _streams = new ConcurrentDictionary<string, FileStream>();
    public static async Task SetStreamAndWait(string user, Stream stream)
    {
        string file = Path.GetTempFileName();
        FileStream newStream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read, 0, FileOptions.Asynchronous);
        _streams.AddOrUpdate(key: user, newStream, updateValueFactory: (_, oldFileName) => 
        {
            oldFileName.Flush();
            oldFileName.Dispose();
            File.Delete(oldFileName.Name);

            return newStream; 
        });

        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(newStream);
    }

    public static Stream? GetStreamForUser(string user)
    {
        if (_streams.TryGetValue(user, out FileStream? stream))
        {
            return new FileStream(stream.Name, FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous);
        }
        else
        {
            return null;
        }
    }
}
