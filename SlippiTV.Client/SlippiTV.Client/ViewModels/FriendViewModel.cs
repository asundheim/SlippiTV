using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;
using Slippi.NET.Types;
using SlippiTV.Client.Settings;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.SocketUtils;
using SlippiTV.Shared.Types;

namespace SlippiTV.Client.ViewModels;

public partial class FriendViewModel : BaseNotifyPropertyChanged
{
    public FriendViewModel(FriendsViewModel parent, FriendSettings friend)
    {
        this.Parent = parent;
        this.FriendSettings = friend;
        this.ActiveGameInfo = new ActiveGameViewModel(this);
    }

    public FriendsViewModel Parent { get; }
    public ISlippiTVService SlippiTVService => Parent.SlippiTVService;
    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;
    public FriendSettings FriendSettings { get; }

    // this shouldn't be bulk updated, it knows how to efficiently update itself
    public ActiveGameViewModel ActiveGameInfo { get; set; }

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

    public async Task Refresh()
    {
        try
        {
            var userInfo = await SlippiTVService.GetStatus(FriendSettings.ConnectCode);
            Parent.ShellViewModel.RelayStatus = LiveStatus.Active;
            CheckForNewStream(userInfo);

            LiveStatus = userInfo.LiveStatus;
            ActiveGameInfo.UpdateGameInfo(userInfo.ActiveGameInfo);
            ViewerCount = userInfo.ActiveViewerInfo?.ActiveViewerCount ?? 0;
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

            bool release = false;
            try
            {
                // allow watching your own stream. this is useful for development and also for people to try it out.
                // it also avoids the issue of streaming the playback dolphin as we must already be connected to the netplay dolphin.
                // the upload loop will keep the lock uncontested and we won't interfere with it.
                if (FriendSettings.ConnectCode != Settings.StreamMeleeConnectCode)
                {
                    // addref
                    int refcount = Parent.ShellViewModel.AddStreamWatcher();
                    release = true;

                    if (refcount == 1)
                    {
                        // kill the upload loop, then take the lock, then re-enter the upload loop waiting on the lock, ensuring we don't race ourselves
                        await Parent.ShellViewModel.DisconnectStream();
                        await Parent.ShellViewModel.StreamLock.WaitAsync(anyCancellation);

                        // kick this off so we trigger a full reconnect that waits on the lock we've taken. we don't want to schedule it on our thread
                        // as it's a mostly synchronous operation
                        _ = Task.Run(Parent.ShellViewModel.ReconnectDolphin);
                    }
                    else
                    {
                        // another watcher already has the lock. we'll still check if we're responsible for releasing it at the end.
                    }
                }

                using var socket = await SlippiTVService.WatchStream(FriendSettings.ConnectCode);
                await SocketUtils.HandleSocket(socket, x => fileWriter.Write(x), null, anyCancellation);
            }
            catch { }
            finally
            {
                if (release)
                {
                    // release
                    int refcount = Parent.ShellViewModel.RemoveStreamWatcher();
                    if (refcount == 0)
                    {
                        Parent.ShellViewModel.StreamLock.Release();
                    }
                }
            }
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

    private int _lastNotified = -1;
    private void CheckForNewStream(UserStatusInfo newStatusInfo)
    {
        if (LiveStatus != LiveStatus.Active && 
            newStatusInfo.LiveStatus == LiveStatus.Active && 
            newStatusInfo.ActiveGameInfo is not null && 
            newStatusInfo.ActiveGameInfo.GameMode == GameMode.ONLINE)
        {
            // breaking the abstraction a bit but :/
            if (_lastNotified == -1 || TimeSpan.FromMilliseconds(Environment.TickCount - _lastNotified).TotalHours >= 1)
            {
                this.Parent.InvokeNewActiveGame(this, newStatusInfo.ActiveGameInfo);
                _lastNotified = Environment.TickCount;
            }
        }
    }
}
