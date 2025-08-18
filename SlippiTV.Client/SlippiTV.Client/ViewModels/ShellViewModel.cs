using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.SocketUtils;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SlippiTV.Client.ViewModels;

public class ShellViewModel : BaseNotifyPropertyChanged
{
    public ShellViewModel() 
    {
        SettingsViewModel = new SettingsViewModel(this);
        FriendsViewModel = new FriendsViewModel(this);

        SlippiTVService = SlippiTVServiceFactory.Instance.GetService();

        ConnectToDolphin();
    }

    public FriendsViewModel FriendsViewModel { get; set; }
    public SettingsViewModel SettingsViewModel { get; set; }

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    public DolphinConnection DolphinConnection { get; set; }
    public ISlippiTVService SlippiTVService { get; set; }

    private CancellationTokenSource _disconnectSource = new CancellationTokenSource();
    private Task? _socketTask = null;
    private async void DolphinConnection_OnStatusChange(object? sender, ConnectionStatus status)
    {
        if (status == ConnectionStatus.Connected)
        {
            _socketTask = Task.Run(async () =>
            {
                try
                {
                    using var socket = await SlippiTVService.Stream(Settings.StreamMeleeConnectCode);
                    var requestClosing = new CancellationTokenSource();
                    var anyCancel = CancellationTokenSource.CreateLinkedTokenSource(_disconnectSource.Token, requestClosing.Token).Token;
                    var sendTask = Task.Run(async () =>
                    {
                        try
                        {
                            await SocketUtils.SendSocket(socket, _pendingData, anyCancel);
                        }
                        finally
                        {
                            requestClosing.Cancel();
                        }
                    });

                    try
                    {
                        await SocketUtils.ReceiveSocket(socket, static x => { }, anyCancel);
                    }
                    finally
                    {
                        requestClosing.Cancel();
                    }

                    await sendTask;

                    await socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Disconnect from Dolphin", CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    switch (ex.WebSocketErrorCode)
                    {
                        case WebSocketError.ConnectionClosedPrematurely:
                            break;

                        default:
                            break;
                    }
                }
            });
        }
        else if (status == ConnectionStatus.Disconnected)
        {
            _disconnectSource.Cancel();
            if (_socketTask is not null)
            {
                await _socketTask;
                _socketTask = null;
            }

            _disconnectSource.Dispose();
            _disconnectSource = new CancellationTokenSource();

            ConnectToDolphin();
        }
    }

    private readonly BlockingCollection<byte[]> _pendingData = new BlockingCollection<byte[]>();
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
}
