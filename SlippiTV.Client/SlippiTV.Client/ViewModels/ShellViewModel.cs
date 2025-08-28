using CommunityToolkit.Mvvm.Input;
using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using SlippiTV.Client.Platforms.Windows;
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

        if (await ClientVersion.RequiresUpdateAsync(viewModel.SlippiTVService))
        {
            viewModel.RequiresUpdate = true;
        }

        return viewModel;
    }

    public FriendsViewModel FriendsViewModel { get; set; }
    public SettingsViewModel SettingsViewModel { get; set; }

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    public DolphinConnection DolphinConnection { get; set; }
    public ISlippiTVService SlippiTVService { get; set; }
    public DolphinRustInvoker? DolphinRustInvoker { get; private set; }

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

    private CancellationTokenSource _disconnectSource = new CancellationTokenSource();
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
            _socketTask = Task.Run(async () =>
            {
                try
                {
                    RelayStatus = LiveStatus.Idle;
                    using var socket = await SlippiTVService.Stream(Settings.StreamMeleeConnectCode);
                    RelayStatus = LiveStatus.Active;
                    await SocketUtils.HandleSocket(socket, null, _pendingData, _disconnectSource.Token);
                }
                catch { }
            });
        }
        else if (status == ConnectionStatus.Disconnected)
        {
            _disconnectSource.Cancel();
            if (_socketTask is not null)
            {
                await _socketTask;
                _socketTask = null;
                RelayStatus = LiveStatus.Offline;
            }

            _disconnectSource.Dispose();
            _disconnectSource = new CancellationTokenSource();

            ConnectToDolphin();
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

    [RelayCommand]
    public void Reconnect()
    {
        DolphinConnection.HandleDisconnect();
    }
}
