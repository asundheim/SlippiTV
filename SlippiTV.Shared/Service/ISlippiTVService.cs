using Refit;

namespace SlippiTV.Shared.Service;

public interface ISlippiTVService
{
    [Multipart]
    [Post("/stream/{user}")]
    Task Stream(string user, Stream FileStream);

    [Get("/stream/{user}")]
    Task<ApiResponse<Stream>> WatchStream(string user);

    [Get("/status/activity/{user}")]
    Task<bool> IsLive(string user);

    [Post("/status/activity/{user}")]
    Task<bool> GoLive(string user);

    [Get("/status/stream/{user}/currentFrame")]
    Task<int> GetCurrentFrameOfStream(string user);

    [Get("/status/watchers/{user}")]
    Task<bool> HasWatchers(string user);
}
