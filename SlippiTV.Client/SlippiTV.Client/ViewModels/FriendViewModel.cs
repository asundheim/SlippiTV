using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.SocketUtils;
using SlippiTV.Shared.Types;
using System.Collections.ObjectModel;

namespace SlippiTV.Client.ViewModels;

public class FriendViewModel : BaseNotifyPropertyChanged
{
    public FriendViewModel(FriendsViewModel parent, string connectCode)
    {
        this.Parent = parent;
        this.ConnectCode = connectCode;
    }

    public FriendsViewModel Parent { get; }

    public ISlippiTVService SlippiTVService => Parent.SlippiTVService;
    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    public string ConnectCode { get; }
    public ActiveGameInfo? ActiveGameInfo 
    { 
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public LiveStatus LiveStatus
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public int ViewerCount
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<int> PlayerStocksLeft { get; } = new ObservableCollection<int>();
    public ObservableCollection<int> OpponentStocksLeft { get; set; } = new ObservableCollection<int>();

    public async Task Refresh()
    {
        try
        {
            var userInfo = await SlippiTVService.GetStatus(ConnectCode);
            Parent.ShellViewModel.RelayStatus = LiveStatus.Active;

            LiveStatus = userInfo.LiveStatus;
            ActiveGameInfo = userInfo.ActiveGameInfo;
            ViewerCount = userInfo.ActiveViewerInfo?.ActiveViewerCount ?? 0;

            if (ActiveGameInfo is not null)
            {
                // it's not smart but it works
                int originalPlayerCount = PlayerStocksLeft.Count;
                if (originalPlayerCount < ActiveGameInfo.PlayerStocksLeft)
                {
                    for (int i = 0; i < ActiveGameInfo.PlayerStocksLeft - originalPlayerCount; i++)
                    {
                        PlayerStocksLeft.Add(i);
                    }
                }
                else if (PlayerStocksLeft.Count > ActiveGameInfo.PlayerStocksLeft)
                {
                    for (int i = 0; i < PlayerStocksLeft.Count - ActiveGameInfo.PlayerStocksLeft; i++)
                    {
                        PlayerStocksLeft.RemoveAt(PlayerStocksLeft.Count - 1);
                    }
                }

                int originalOpponentCount = OpponentStocksLeft.Count;
                if (originalOpponentCount < ActiveGameInfo.OpponentStocksLeft)
                {
                    for (int i = 0; i < ActiveGameInfo.OpponentStocksLeft - originalOpponentCount; i++)
                    {
                        OpponentStocksLeft.Add(i);
                    }
                }
                else if (OpponentStocksLeft.Count > ActiveGameInfo.OpponentStocksLeft)
                {
                    for (int i = 0; i < originalOpponentCount - ActiveGameInfo.OpponentStocksLeft; i++)
                    {
                        OpponentStocksLeft.RemoveAt(OpponentStocksLeft.Count - 1);
                    }
                }
            }
            else
            {
                PlayerStocksLeft.Clear();
                OpponentStocksLeft.Clear();
            }
        }
        catch
        {
            Parent.ShellViewModel.RelayStatus = LiveStatus.Offline;
        }
    }

    public async Task Watch(CancellationToken cancellation)
    {
        if (LiveStatus != LiveStatus.Active)
        {
            throw new InvalidOperationException();
        }

        if (!Path.Exists(Settings.WatchDolphinPath) || !Path.Exists(Settings.WatchMeleeISOPath))
        {
            // TODO show message box
            return;
        }

        DolphinLauncher? launcher = null;
        SlpFileWriter fileWriter = new SlpFileWriter(new SlpFileWriterSettings
        {
            FolderPath = Path.GetTempPath()
        });
        CancellationTokenSource dolphinCloseSource = new CancellationTokenSource();
        CancellationToken anyCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation, dolphinCloseSource.Token).Token;
        string? currentFile = null;
        try
        {
            fileWriter.OnNewFile += (object? sender, string newFile) =>
            {
                if (launcher is null)
                {
                    launcher = new DolphinLauncher(Settings.WatchMeleeISOPath, Settings.WatchDolphinPath);
                    launcher.OnDolphinClosed += (o, e) =>
                    {
                        dolphinCloseSource.Cancel();
                    };
                }
                
                launcher.LaunchDolphin(new DolphinLaunchArgs()
                {
                    Mode = DolphinLaunchModes.Mirror,
                    Replay = newFile,
                    IsRealTimeMode = false,
                    GameStation = "SlippiTV"
                });

                try
                {
                    if (currentFile is not null && File.Exists(currentFile))
                    {
                        File.Delete(currentFile);
                    }
                }
                catch { }

                currentFile = newFile;
            };

            try
            {
                using var socket = await SlippiTVService.WatchStream(ConnectCode);
                await SocketUtils.HandleSocket(socket, x => fileWriter.Write(x), null, anyCancellation);
            }
            catch { }
        }
        finally
        {
            fileWriter.Dispose();
            launcher?.Dispose();
            dolphinCloseSource.Dispose();

            try
            {
                if (currentFile is not null && File.Exists(currentFile))
                {
                    File.Delete(currentFile);
                }
            }
            catch { } // best effort
        }
    }
}
