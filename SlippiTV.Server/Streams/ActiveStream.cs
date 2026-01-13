using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using Slippi.NET.Utils;
using SlippiTV.Shared;
using SlippiTV.Shared.Types;
using System.Collections.Concurrent;

namespace SlippiTV.Server.Streams;

public class ActiveStream : IDisposable
{
    public ActiveStream(string username)
    {
        Username = username;
        ConnectCode = ConnectCodeUtils.UnsanitizeConnectCode(username);

        SlpEventStream = new SlpEventStream(new SlpStreamSettings()
        {
            Mode = SlpStreamModes.AUTO
        });
        SlpEventStream.OnCommand += SlpEventStream_OnCommand;
    }

    private PlayerIndices? _playerIndices = null;
    private void SlpEventStream_OnCommand(object? sender, SlpStreamCommandEventArgs e)
    {
        if (e.Command == Command.GAME_END)
        {
            IsActive = false;
            ActiveGameInfo = null;
            EndGame();

            if (_playerIndices is not null && _lastGameStart is not null && e.Payload is GameEndPayload payload)
            {
                List<PostFrameUpdate> finalPostFrameUpdates = [];
                if (_lastPlayerPostFrameUpdate is not null)
                {
                    finalPostFrameUpdates.Add(_lastPlayerPostFrameUpdate);
                }

                if (_lastOpponentPostFrameUpdate is not null)
                {
                    finalPostFrameUpdates.Add(_lastOpponentPostFrameUpdate);
                }

                Placement? winner = WinnerCalculator.GetWinners(payload.GameEnd, _lastGameStart, finalPostFrameUpdates).FirstOrDefault();
                if (winner is not null)
                {
                    if (winner.PlayerIndex == _playerIndices.PlayerIndex)
                    {
                        _playerGamesWon++;
                    }
                    else if (winner.PlayerIndex == _playerIndices.OpponentIndex)
                    {
                        _opponentGamesWon++;
                    }
                }
            }
        }
        else if (e.Command == Command.GAME_START)
        {
            IsActive = true;
            
            if (e.Payload is GameStartPayload payload)
            {
                // try to put ourselves as Player, unless neither player is Player
                List<Player> players = payload.GameStart.Players.Where(p => p.Type == 0).ToList();
                for (int i = 0; i < 2; i++)
                {
                    Player playerInfo = players[i];
                    Player opponentInfo = players[i ^ 1]; // p = 0, 0 ^ 1 => 1; p = 1, 1 ^ 1 => 0
                    string? connectCode = playerInfo.ConnectCode;
                    if (connectCode == ConnectCode || i == 1)
                    {
                        // either we found it or we're assigning this arbitrarily
                        _playerIndices = new PlayerIndices()
                        {
                            PlayerIndex = i,
                            OpponentIndex = i ^ 1
                        };

                        if (connectCode != _playerConnectCode || opponentInfo.ConnectCode != _opponentConnectCode)
                        {
                            _playerGamesWon = 0;
                            _opponentGamesWon = 0;
                        }

                        _playerConnectCode = connectCode;
                        _opponentConnectCode = opponentInfo.ConnectCode;
                        ActiveGameInfo = new ActiveGameInfo()
                        {
                            GameNumber = (int)(payload.GameStart.MatchInfo?.GameNumber ?? 1),
                            Stage = payload.GameStart.Stage!.Value,
                            GameMode = payload.GameStart.GameMode!.Value,
                            PlayerConnectCode = _playerConnectCode ?? string.Empty,
                            PlayerDisplayName = playerInfo.DisplayName ?? string.Empty,
                            PlayerCharacter = playerInfo.Character!.Value,
                            PlayerCharacterColor = playerInfo.CharacterColor!.Value,
                            PlayerStocksLeft = (byte)(playerInfo.StartStocks ?? 4),
                            OpponentConnectCode = _opponentConnectCode ?? string.Empty,
                            OpponentDisplayName = opponentInfo.DisplayName ?? string.Empty,
                            OpponentCharacter = opponentInfo.Character!.Value,
                            OpponentCharacterColor = opponentInfo.CharacterColor!.Value,
                            OpponentStocksLeft = (byte)(opponentInfo.StartStocks ?? 4),
                            PlayerGamesWon = _playerGamesWon,
                            OpponentGamesWon = _opponentGamesWon
                        };

                        break;
                    }
                }

                _lastGameStart = payload.GameStart;
            }
        }
        else if (e.Command == Command.POST_FRAME_UPDATE)
        {
            if (e.Payload is PostFrameUpdatePayload payload)
            {
                if (_playerIndices is not null && ActiveGameInfo is not null /* really shouldn't ever be... */)
                {
                    if (payload.PostFrameUpdate.PlayerIndex == _playerIndices.PlayerIndex)
                    {
                        ActiveGameInfo.PlayerStocksLeft = payload.PostFrameUpdate.StocksRemaining!.Value;
                        _lastPlayerPostFrameUpdate = payload.PostFrameUpdate;
                    }
                    else if (payload.PostFrameUpdate.PlayerIndex == _playerIndices.OpponentIndex)
                    {
                        ActiveGameInfo.OpponentStocksLeft = payload.PostFrameUpdate.StocksRemaining!.Value;
                        _lastOpponentPostFrameUpdate = payload.PostFrameUpdate;
                    }
                }
            }
        }
    }

    public string Username { get; }
    public string ConnectCode { get; }

    public SlpEventStream SlpEventStream { get; }

    public bool IsActive { get; private set; } = false;
    public ActiveGameInfo? ActiveGameInfo { get; private set; } = null;

    private int _watcherCount = 0;
    public int WatcherCount => _watcherCount;

    public bool Disposed { get; set; } = false;

    private readonly Lock _consumersLock = new Lock();
    private readonly List<BlockingCollection<byte[]>> _consumers = [];

    private readonly Lock _currentGameDataLock = new Lock();
    private List<byte[]> _currentGameData = [];

    private string? _playerConnectCode;
    private string? _opponentConnectCode;
    private int _playerGamesWon = 0;
    private int _opponentGamesWon = 0;
    private GameStart? _lastGameStart;
    private PostFrameUpdate? _lastPlayerPostFrameUpdate;
    private PostFrameUpdate? _lastOpponentPostFrameUpdate;

    public Dictionary<int, string>? NameOverrides { get; set; }

    public BlockingCollection<byte[]> GetDataStream()
    {
        BlockingCollection<byte[]> newStream = new BlockingCollection<byte[]>();

        lock (_consumersLock)
        {
            _consumers.Add(newStream);
        }

        lock (_currentGameDataLock)
        {
            foreach (var data in _currentGameData)
            {
                newStream.Add(data);
            }
        }

        return newStream;
    }

    public void WriteData(byte[] data)
    {
        // First, write to the current game stream
        lock (_currentGameDataLock)
        {
            _currentGameData.Add(data);
        }

        // Now write to all active consumers
        lock (_consumersLock)
        {
            foreach (var consumer in _consumers)
            {
                consumer.Add(data);
            }
        }

        // Finally, write to the event stream
        SlpEventStream.Write(data);
    }

    public void AddWatcher()
    {
        Interlocked.Increment(ref _watcherCount);
    }

    public void RemoveWatcher()
    {
        Interlocked.Decrement(ref _watcherCount);
    }

    private void EndGame()
    {
        lock (_currentGameDataLock)
        {
            _currentGameData = [];
        }
    }

    private void CloseAllStreams()
    {
        lock (_consumersLock)
        {
            foreach (var consumer in _consumers)
            {
                consumer.CompleteAdding();
            }
        }
    }

    public void Dispose()
    {
        SlpEventStream.OnCommand -= SlpEventStream_OnCommand;
        Disposed = true;

        CloseAllStreams();
    }
}
