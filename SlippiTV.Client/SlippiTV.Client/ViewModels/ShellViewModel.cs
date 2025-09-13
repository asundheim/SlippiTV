using CommunityToolkit.Mvvm.Input;
using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using SlippiTV.Client.Platforms.Windows;
using SlippiTV.Client.Settings;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.SocketUtils;
using SlippiTV.Shared.Types;
using SlippiTV.Shared.Versions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client.ViewModels;

public partial class ShellViewModel : BaseNotifyPropertyChanged
{
    private ShellViewModel() 
    {
        SlippiTVService = SlippiTVServiceFactory.Instance.GetService();

        SettingsViewModel = new SettingsViewModel(this);
        FriendsViewModel = new FriendsViewModel(this);

        ConnectToDolphin();
    }

    public static async Task<ShellViewModel> CreateAsync()
    {
        await Task.Yield();
        var viewModel = new ShellViewModel();

        if (!string.IsNullOrEmpty(viewModel.Settings.SlippiLauncherFolder) && 
            !string.IsNullOrEmpty(viewModel.Settings.WatchMeleeISOPath) &&
            !string.IsNullOrEmpty(viewModel.Settings.SlippiVersion))
        {
            //viewModel.DolphinRustInvoker = await DolphinRustInvoker.CreateAsync(
            //    viewModel.Settings.WatchMeleeISOPath,
            //    Path.Join(viewModel.Settings.SlippiLauncherFolder, "netplay", "User", "Slippi"),
            //    viewModel.Settings.SlippiVersion);
        }

        viewModel.RequiresUpdate = await ClientVersion.RequiresUpdateAsync(viewModel.SlippiTVService);

        return viewModel;
    }

    public FriendsViewModel FriendsViewModel { get; set; }
    public SettingsViewModel SettingsViewModel { get; set; }

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    public DolphinConnection DolphinConnection { get; set; }
    public ISlippiTVService SlippiTVService { get; set; }
    public DolphinRustInvoker? DolphinRustInvoker { get; private set; }

    public SemaphoreSlim StreamLock = new SemaphoreSlim(1, 1);

    // a ref-count for stream watchers. this ensures multiple streams can be watched at the same time and that the last one always releases the lock
    private int _streamWatchers = 0;
    public int AddStreamWatcher()
    {
        return Interlocked.Increment(ref _streamWatchers);
    }

    public int RemoveStreamWatcher()
    {
        return Interlocked.Decrement(ref _streamWatchers);
    }

    public bool RequiresUpdate
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

    public LiveStatus DolphinStatus
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
    } = LiveStatus.Offline;

    public LiveStatus RelayStatus
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
    } = LiveStatus.Offline;

    public bool AnimateRelayStatus
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

    // TODO extract this all out into some connection manager

    private CancellationTokenSource _dolphinDisconnectSource = new CancellationTokenSource();
    private CancellationTokenSource _socketDisconnectSource = new CancellationTokenSource();
    private Task? _socketTask = null;
    private async void DolphinConnection_OnStatusChange(object? sender, ConnectionStatus status)
    {
        DolphinStatus = status switch
        {
            ConnectionStatus.Connected => LiveStatus.Active,
            ConnectionStatus.Disconnected => LiveStatus.Offline,
            ConnectionStatus.Connecting => LiveStatus.Idle,
            _ => LiveStatus.Offline
        };

        if (status == ConnectionStatus.Connected)
        {
            _socketDisconnectSource.Cancel();
            if (_socketTask is not null)
            {
                await _socketTask;
                _socketTask = null;
            }

            StartSocket();
        }
        else if (status == ConnectionStatus.Disconnected)
        {
            try
            {
                _dolphinDisconnectSource.Cancel();
                if (_socketTask is not null)
                {
                    await _socketTask;
                    _socketTask = null;
                }
            }
            catch { }
            finally
            {
                _dolphinDisconnectSource.Dispose();
                _dolphinDisconnectSource = new CancellationTokenSource();

                ConnectToDolphin();
            }
        }
    }

    private BlockingCollection<byte[]> _pendingData = new BlockingCollection<byte[]>();
    private void DolphinConnection_OnData(object? sender, byte[] e)
    {
        _pendingData.Add(e);
    }

    [MemberNotNull(nameof(DolphinConnection))]
    private void ConnectToDolphin()
    {
        if (DolphinConnection is not null)
        {
            DolphinConnection.Dispose();
            DolphinConnection.OnStatusChange -= DolphinConnection_OnStatusChange;
            DolphinConnection.OnData -= DolphinConnection_OnData;
        }

        DolphinConnection = new DolphinConnection();
        DolphinConnection.OnStatusChange += DolphinConnection_OnStatusChange;
        DolphinConnection.OnData += DolphinConnection_OnData;

        _pendingData?.Dispose();
        _pendingData = new BlockingCollection<byte[]>();
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    DolphinConnection.Connect("127.0.0.1", (int)Ports.Default, isRealtime: true, timeout: 1000);
                    break;
                }
                catch
                {
                    await Task.Delay(5000);
                }
            }
        });
    }

    private void StartSocket()
    {
        _socketDisconnectSource.Dispose();
        _socketDisconnectSource = new CancellationTokenSource();
        CancellationToken anyCancellation = CancellationTokenSource.CreateLinkedTokenSource(_dolphinDisconnectSource.Token, _socketDisconnectSource.Token).Token;

        _socketTask = Task.Run(async () =>
        {
            while (!anyCancellation.IsCancellationRequested)
            {
                bool entered = false;
                try
                {
                    await StreamLock.WaitAsync(anyCancellation);
                    entered = true;
                    RelayStatus = LiveStatus.Idle;
                    using var socket = await SlippiTVService.Stream(Settings.StreamMeleeConnectCode);
                    RelayStatus = LiveStatus.Active;
                    await SocketUtils.HandleSocket(socket, null, _pendingData, anyCancellation);
                }
                catch { }   
                finally
                {
                    if (entered)
                    {
                        StreamLock.Release();
                    }
                }
            }
        });
    }

    // resets all the connections
    [RelayCommand]
    public void ReconnectDolphin()
    {
        DolphinConnection.HandleDisconnect();
    }

    /// <returns>If a stream was active to be disconnected.</returns>
    public bool DisconnectStream()
    {
        if (_socketTask is not null)
        {
            _socketDisconnectSource.Cancel();
            return true;
        }

        return false;
    }

    public async Task ReconnectStream()
    {
        // Presumably we're here after DisconnectStream() was called, but just in case
        // TODO can this just be all wrapped into StartSocket()?
        _socketDisconnectSource.Cancel();

        if (_socketTask is not null)
        {
            await _socketTask;
            _socketTask = null;
        }

        StartSocket();
    }
}
