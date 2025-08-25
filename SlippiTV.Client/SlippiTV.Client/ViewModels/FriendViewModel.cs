using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.SocketUtils;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;

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
            field = value;
            OnPropertyChanged();
        }
    }

    public LiveStatus LiveStatus
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public async Task Refresh()
    {
        try
        {
            LiveStatus = await SlippiTVService.GetStatus(ConnectCode);
            Parent.ShellViewModel.RelayStatus = LiveStatus.Active;

            if (LiveStatus == LiveStatus.Active)
            {
                var gameInfo = await SlippiTVService.GetActiveGameInfo(ConnectCode);
                ActiveGameInfo = gameInfo;
            }
            else
            {
                ActiveGameInfo = null;
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
        string? currentFile = null;
        try
        {
            fileWriter.OnNewFile += (object? sender, string newFile) =>
            {
                launcher ??= new DolphinLauncher(Settings.WatchMeleeISOPath, Settings.WatchDolphinPath);
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
                await SocketUtils.HandleSocket(socket, x => fileWriter.Write(x), null, cancellation);
            }
            catch { }
        }
        finally
        {
            fileWriter.Dispose();
            launcher?.Dispose();

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
