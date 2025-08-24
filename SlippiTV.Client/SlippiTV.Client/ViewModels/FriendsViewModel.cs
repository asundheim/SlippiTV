using H.NotifyIcon;
using SlippiTV.Shared.Service;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        Settings.Friends.CollectionChanged += SettingsFriendsChanged;
        Friends = new ObservableCollection<FriendViewModel>();
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
                    if (myStatus == LiveStatus.Active)
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

    private void SettingsFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            Friends.Add(new FriendViewModel(this, (string)e.NewItems![0]!));
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            Friends.RemoveAt(e.OldStartingIndex);
        }
    }

    public ISlippiTVService SlippiTVService => ShellViewModel.SlippiTVService;

    public ObservableCollection<FriendViewModel> Friends
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    private void CreateFriendsFromSettings()
    {
        foreach (var friend in Settings.Friends)
        {
            Friends.Add(new FriendViewModel(this, friend));
        }
    }

    [RelayCommand]
    public void ShowHideWindow()
    {
        var window = Application.Current?.Windows[0];
        if (window == null)
        {
            return;
        }

        if (window.IsActivated())
        {
            window.Hide(enableEfficiencyMode: true);
        }
        else
        {
            window.Show(disableEfficiencyMode: true);
        }
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