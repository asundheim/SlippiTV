using Newtonsoft.Json;

namespace SlippiTV.Shared.Types;

[JsonObject]
public class ActiveViewerInfo
{
    public int ActiveViewerCount { get; set; } = 0;
}
