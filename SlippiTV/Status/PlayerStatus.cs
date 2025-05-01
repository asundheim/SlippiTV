namespace SlippiTV.Status;

public class PlayerStatus
{
    private int _isLive = 0;
    public bool IsLive => _isLive != 0;

    private int _watcherCount = 0;
    public int WatcherCount => _watcherCount;

    public void AddWatcher()
    {
        Interlocked.Increment(ref _watcherCount);
    }

    public void RemoveWatcher()
    {
        Interlocked.Decrement(ref _watcherCount);
    }

    public void GoLive()
    {
        Interlocked.Exchange(ref _isLive, 1);
    }

    public void StopLive()
    {
        Interlocked.Exchange(ref _isLive, 0);
    }
}
