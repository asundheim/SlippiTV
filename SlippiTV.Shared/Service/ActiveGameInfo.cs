using Newtonsoft.Json;

namespace SlippiTV.Shared.Service;

[JsonObject]
public class ActiveGameInfo
{
    public string PlayerConnectCode { get; set; } = string.Empty;

    public string PlayerDisplayName { get; set; } = string.Empty;

    public string OpponentConnectCode { get; set; } = string.Empty;

    public string OpponentDisplayName { get; set; } = string.Empty;

    public int GameNumber { get; set; } = 1;
}
