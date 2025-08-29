using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
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
            EndGame();
        }
        else if (e.Command == Command.GAME_START)
        {
            IsActive = true;
            
            if (e.Payload is GameStartPayload payload)
            {
                // try to put ourselves as Player, unless neither player is Player
                for (int i = 0; i < 2; i++)
                {
                    
                    Player playerInfo = payload.GameStart.Players[i];
                    Player opponentInfo = payload.GameStart.Players[i ^ 1]; // p = 0, 0 ^ 1 => 1; p = 1, 1 ^ 1 => 0
                    string? connectCode = playerInfo.ConnectCode;
                    if (connectCode == ConnectCode || i == 1)
                    {
                        // either we found it or we're assigning this arbitrarily
                        _playerIndices = new PlayerIndices()
                        {
                            PlayerIndex = i,
                            OpponentIndex = i ^ 1
                        };

                        ActiveGameInfo = new ActiveGameInfo()
                        {
                            GameNumber = (int)(payload.GameStart.MatchInfo?.GameNumber ?? 1),
                            Stage = payload.GameStart.Stage!.Value,
                            GameMode = payload.GameStart.GameMode!.Value,
                            PlayerConnectCode = connectCode ?? string.Empty,
                            PlayerDisplayName = playerInfo.DisplayName ?? string.Empty,
                            PlayerCharacter = playerInfo.Character!.Value,
                            PlayerCharacterColor = playerInfo.CharacterColor!.Value,
                            PlayerStocksLeft = 4,
                            OpponentConnectCode = opponentInfo.ConnectCode ?? string.Empty,
                            OpponentDisplayName = opponentInfo.DisplayName ?? string.Empty,
                            OpponentCharacter = opponentInfo.Character!.Value,
                            OpponentCharacterColor = opponentInfo.CharacterColor!.Value,
                            OpponentStocksLeft = 4
                        };

                        break;
                    }
                }
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
                    }
                    else if (payload.PostFrameUpdate.PlayerIndex == _playerIndices.OpponentIndex)
                    {
                        ActiveGameInfo.OpponentStocksLeft = payload.PostFrameUpdate.StocksRemaining!.Value;
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
