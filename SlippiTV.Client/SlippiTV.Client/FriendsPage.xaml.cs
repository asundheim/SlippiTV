using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Shapes;
using SlippiTV.Client.ViewModels;
using SlippiTV.Shared;
using SlippiTV.Shared.Types;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client;

public partial class FriendsPage : ContentPage
{
    [NotNull]
    public FriendsViewModel? FriendsViewModel
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public FriendsPage()
    {
        InitializeComponent();
        Loaded += FriendsPage_Loaded;
    }

    private void FriendsPage_Loaded(object? sender, EventArgs e)
    {
        FriendsViewModel = (FriendsViewModel)this.BindingContext;
    }

    private async void AddFriendsButton_Clicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<string>(
            new InputTextPopup(
                placeholderText: "Enter Connect Code (ABC#123)",
                title: "Input Connect Code"
                ), 
            new PopupOptions 
            { 
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(0),
                    StrokeThickness = 0,
                } 
            });
        if (result.WasDismissedByTappingOutsideOfPopup || string.IsNullOrEmpty(result.Result))
        {
            return;
        }

        SettingsManager.Instance.AddFriend(result.Result);
    }

    private async void WatchFriendButton_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is FriendViewModel friend)
        {
            if (string.IsNullOrEmpty(FriendsViewModel.Settings.WatchDolphinPath) || string.IsNullOrEmpty(FriendsViewModel.Settings.WatchMeleeISOPath))
            {
                await this.ShowPopupAsync(new ErrorPopup("Invalid replay Dolphin path or SSBM .iso path"), new PopupOptions
                {
                    Shape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(0),
                        StrokeThickness = 0,
                    }
                });
            }
            else
            {
                await friend.Watch(CancellationToken.None);
            }
        }
    }

    private void FriendsMore_Tapped(object sender, TappedEventArgs e)
    {
        var view = sender as View;

        MenuFlyout mf = new MenuFlyout();
        MenuFlyoutItem flyoutItem = new MenuFlyoutItem();
        flyoutItem.Text = "Remove Friend";
        flyoutItem.Command = new RelayCommand(() =>
        {
            SettingsManager.Instance.RemoveFriend(((FriendViewModel)view!.BindingContext).ConnectCode);
        });
        mf.Add(flyoutItem);
        FlyoutBase.SetContextFlyout(view, mf);

        var point = e.GetPosition(view);
        PlatformUtils.PlatformUtils.ShowContextMenu(view, point);
    }

    private async void AddFromRecentButton_Clicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<string>(
            new SelectableListViewPopup(SettingsManager.Instance.AddFromRecentCandidates),
            new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(0),
                    StrokeThickness = 0,
                }
            });
        if (result.WasDismissedByTappingOutsideOfPopup || string.IsNullOrEmpty(result.Result))
        {
            return;
        }

        var codes = result.Result.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var code in codes)
        {
            SettingsManager.Instance.AddFriend(code);
        }
    }

    private void OpenOpponentWebpage(object sender, EventArgs e)
    {
        FriendViewModel friend = (FriendViewModel)((VisualElement)sender).BindingContext;
        if (!string.IsNullOrEmpty(friend.ActiveGameInfo?.OpponentConnectCode))
        {
            string sanitized = ConnectCodeUtils.SanitizeConnectCode(friend.ActiveGameInfo.OpponentConnectCode);
            Process.Start(new ProcessStartInfo($"https://slippi.gg/user/{sanitized}") { UseShellExecute = true });
        }

        return;
    }

    private void OpenPlayerWebpage(object sender, EventArgs e)
    {
        FriendViewModel friend = (FriendViewModel)((VisualElement)sender).BindingContext;
        if (!string.IsNullOrEmpty(friend.ActiveGameInfo?.PlayerConnectCode))
        {
            string sanitized = ConnectCodeUtils.SanitizeConnectCode(friend.ActiveGameInfo.PlayerConnectCode);
            Process.Start(new ProcessStartInfo($"https://slippi.gg/user/{sanitized}") { UseShellExecute = true });
        }

        return;
    }

    private void NotificationBell_Tapped(object sender, TappedEventArgs e)
    {
        return;
    }
}
