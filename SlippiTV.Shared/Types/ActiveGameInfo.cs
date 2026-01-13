using Newtonsoft.Json;
using Slippi.NET.Melee.Types;
using Slippi.NET.Types;

namespace SlippiTV.Shared.Types;

[JsonObject]
public class ActiveGameInfo
{
    public string PlayerConnectCode { get; set; } = string.Empty;

    public string PlayerDisplayName { get; set; } = string.Empty;

    public required Character PlayerCharacter { get; set; }

    public required byte PlayerCharacterColor { get; set; }

    public required byte PlayerStocksLeft { get; set; }

    public string OpponentConnectCode { get; set; } = string.Empty;

    public string OpponentDisplayName { get; set; } = string.Empty;

    public required Character OpponentCharacter { get; set; }

    public required byte OpponentCharacterColor { get; set; }

    public required byte OpponentStocksLeft { get; set; }

    public required int GameNumber { get; set; }

    public required Stage Stage { get; set; }
    public required GameMode GameMode { get; set; }
    public required int PlayerGamesWon { get; set; }
    public required int OpponentGamesWon { get; set; }
}
