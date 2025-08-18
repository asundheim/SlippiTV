using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using SlippiTV.Client.ViewModels;
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
                placeholderText: "Enter Connect Code",
                title: "Input Connect Code (ABC#123)"
            ), 
            PopupOptions.Empty);
        if (result.WasDismissedByTappingOutsideOfPopup || string.IsNullOrEmpty(result.Result))
        {
            return;
        }

        SettingsManager.Instance.AddFriend(result.Result);
    }

    private void RemoveFriendButton_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is FriendViewModel friend)
        {
            SettingsManager.Instance.RemoveFriend(friend.ConnectCode);
        }
    }

    private async void WatchFriendButton_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is FriendViewModel friend)
        {
            await friend.Watch(CancellationToken.None);
        }
    }
}
