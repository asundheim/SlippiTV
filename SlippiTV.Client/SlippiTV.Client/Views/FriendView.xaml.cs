using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Input;
using MauiIcons.Core;
using MauiIcons.Fluent.Filled;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json.Linq;
using SlippiTV.Client.Pages;
using SlippiTV.Client.ViewModels;
using SlippiTV.Shared;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client.Views;

public partial class FriendView : ContentView
{
    [AllowNull]
    public FriendViewModel FriendViewModel { get; set; }

    [AllowNull]
    public FriendsPage FriendsPage { get; set; }

	public FriendView()
	{
		InitializeComponent();
        Loaded += FriendView_Loaded;
	}

    private void FriendView_Loaded(object? sender, EventArgs e)
    {
        this.FriendViewModel = (FriendViewModel)this.BindingContext;
        BellNotificationColor();
    }

    public void BellNotificationColor()
    {
        var bellIcon = NotificationBellIcon;
        var notifStatus = FriendViewModel.FriendSettings.NotificationsEnabled;

        if (Application.Current is Application application)
        {
            var colorOption = notifStatus ? (Color)Application.Current.Resources["NotificationPrimary"] : (Color)Application.Current.Resources["NotificationSecondary"];
            bellIcon.Color = colorOption;
        }
    }

    private async void WatchFriendButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(FriendViewModel.Settings.WatchDolphinPath) || string.IsNullOrEmpty(FriendViewModel.Settings.WatchMeleeISOPath))
        {
            await FriendViewModel.Parent.ShowErrorPopupAsync(this, new TextPopupEventArgs("Invalid replay Dolphin path or SSBM .iso path", new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(0),
                    StrokeThickness = 0,
                }
            }));
        }
        else
        {
            await FriendViewModel.Watch(CancellationToken.None);
        }
    }

    private void FriendsMore_Tapped(object sender, TappedEventArgs e)
    {
        var view = sender as View;

        MenuFlyout mf = new MenuFlyout();

        MenuFlyoutItem renameItem = new MenuFlyoutItem();
        renameItem.Text = string.IsNullOrEmpty(FriendViewModel.FriendSettings.Nickname) ? "Set nickname" : "Rename";
        MauiIcon renameIcon = new MauiIcon() { Icon = FluentFilledIcons.Edit24Filled };
        renameItem.IconImageSource = (ImageSource)renameIcon; // I think this is actually what they want you to do?
        renameItem.Command = new RelayCommand(async () =>
        {
            var result = await Shell.Current.CurrentPage.ShowPopupAsync<string>(
                new InputTextPopup("Set a nickname for this friend", "Set Nickname"),
                new PopupOptions
                {
                    Shape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(0),
                        StrokeThickness = 0,
                    }
                });
            if (!string.IsNullOrEmpty(result.Result) && !result.WasDismissedByTappingOutsideOfPopup)
            {
                FriendViewModel.RenameFriend(result.Result);
            }
        });
        mf.Add(renameItem);

        MenuFlyoutItem removeItem = new MenuFlyoutItem();
        removeItem.Text = "Remove Friend";
        MauiIcon removeIcon = new MauiIcon() { Icon = FluentFilledIcons.Delete24Filled };
        removeItem.IconImageSource = (FontImageSource)removeIcon;
        removeItem.Command = new RelayCommand(() =>
        {
            FriendViewModel.Parent.RemoveFriend(FriendViewModel);
        });
        mf.Add(removeItem);

        FlyoutBase.SetContextFlyout(view, mf);

        var point = e.GetPosition(view);
        PlatformUtils.PlatformUtils.ShowContextMenu(view, point);
    }

    private void OpenOpponentWebpage(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(FriendViewModel.ActiveGameInfo.OpponentConnectCode))
        {
            string sanitized = ConnectCodeUtils.SanitizeConnectCode(FriendViewModel.ActiveGameInfo.OpponentConnectCode);
            Process.Start(new ProcessStartInfo($"https://slippi.gg/user/{sanitized}") { UseShellExecute = true });
        }

        return;
    }

    private void OpenPlayerWebpage(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(FriendViewModel.ActiveGameInfo.PlayerConnectCode))
        {
            string sanitized = ConnectCodeUtils.SanitizeConnectCode(FriendViewModel.ActiveGameInfo.PlayerConnectCode);
            Process.Start(new ProcessStartInfo($"https://slippi.gg/user/{sanitized}") { UseShellExecute = true });
        }

        return;
    }

    private void NotificationBell_Tapped(object sender, TappedEventArgs e)
    {
        FriendViewModel.FriendSettings.NotificationsEnabled = !FriendViewModel.FriendSettings.NotificationsEnabled;
        BellNotificationColor();
    }
}