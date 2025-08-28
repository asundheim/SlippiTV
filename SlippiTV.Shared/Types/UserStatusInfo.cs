using Newtonsoft.Json;

namespace SlippiTV.Shared.Types;

[JsonObject]
public class UserStatusInfo
{
    public LiveStatus LiveStatus { get; set; } = LiveStatus.Offline;

    public ActiveGameInfo? ActiveGameInfo { get; set; } = null;

    public ActiveViewerInfo? ActiveViewerInfo { get; set; } = null; 
}
