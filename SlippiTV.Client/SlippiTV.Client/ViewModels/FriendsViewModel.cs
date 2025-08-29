using H.NotifyIcon;
using SlippiTV.Shared.Service;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;
using SlippiTV.Shared.Types;
using SlippiTV.Shared.Versions;
using SlippiTV.Client.Settings;
using SlippiTV.Shared;

namespace SlippiTV.Client.ViewModels;

public partial class FriendsViewModel : BaseNotifyPropertyChanged
{
    public ShellViewModel ShellViewModel
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public FriendsViewModel(ShellViewModel shellViewModel)
    {
        this.ShellViewModel = shellViewModel;
        CreateFriendsFromSettings();

        _ = Task.Run(async () =>
        {
            while (true)
            {
                foreach (var friend in Friends)
                {
                    await friend.Refresh();
                }

                // Check on ourselves
                try
                {
                    var myStatus = await SlippiTVService.GetStatus(Settings.StreamMeleeConnectCode);
                    if (myStatus.LiveStatus == LiveStatus.Active)
                    {
                        ShellViewModel.AnimateRelayStatus = true;
                    }
                    else
                    {
                        ShellViewModel.AnimateRelayStatus = false;
                    }
                }
                catch { }

                await Task.Delay(5000);
            }
        });
    }

    public bool TryAddFriend(string connectCode, out FriendViewModel friend)
    {
        if (SettingsManager.Instance.TryCreateFriend(connectCode, out FriendSettings friendSettings))
        {
            friend = new FriendViewModel(this, friendSettings);
            Friends.Add(friend);
            return true;
        }
        else
        {
            friend = Friends.First(f => f.FriendSettings.ConnectCode == friendSettings.ConnectCode);
            return false;
        }
    }

    public void RemoveFriend(FriendViewModel friend)
    {
        SettingsManager.Instance.RemoveFriend(friend.FriendSettings);
        Friends.Remove(friend);
    }

    /// <summary>
    /// Adds a friend by code if not present, refreshes, and watches if live.
    /// </summary>
    public async Task WatchByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        code = ConnectCodeUtils.NormalizeConnectCode(code.Trim());
        TryAddFriend(code, out FriendViewModel friend);

        await friend.Refresh();
        try
        {
            await friend.Watch(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // not live
        }
    }

    public event EventHandler<FriendViewModel, ActiveGameInfo>? OnNewActiveGame;
    internal void InvokeNewActiveGame(FriendViewModel friend, ActiveGameInfo gameInfo) => OnNewActiveGame?.Invoke(friend, gameInfo);

    public ISlippiTVService SlippiTVService => ShellViewModel.SlippiTVService;

    public ObservableCollection<FriendViewModel> Friends
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new ObservableCollection<FriendViewModel>();

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    private void CreateFriendsFromSettings()
    {
        foreach (var friend in Settings.Friends)
        {
            Friends.Add(new FriendViewModel(this, friend));
        }
    }

    [RelayCommand]
    public async Task ShowWindow()
    {
        // as good a place to check as any unless we dedicate some polling thread to it
        this.ShellViewModel.RequiresUpdate = await ClientVersion.RequiresUpdateAsync(SlippiTVService);

        var window = Application.Current?.Windows[0];
        if (window == null)
        {
            return;
        }

        window.Show(disableEfficiencyMode: true);
    }

    [RelayCommand]
    public void ExitApplication()
    {
        Application.Current?.Quit();
    }
}

file static class Accessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsActivated")]
    public static extern bool IsActivated(this Window window);
}
