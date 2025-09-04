using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using Slippi.NET.Melee;
using Slippi.NET.Melee.Types;
using SlippiTV.Client.ViewModels;
using SlippiTV.Client.Views;
using SlippiTV.Shared.Types;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client.Pages;

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
        FriendsViewModel.OnShowErrorPopup += FriendsViewModel_OnShowErrorPopup;
        FriendsViewModel.OnNewActiveGame += OnNewActiveGame;
    }

    private async Task FriendsViewModel_OnShowErrorPopup(object sender, TextPopupEventArgs args)
    {
        await this.ShowPopupAsync(new ErrorPopup(args.PopupContent), args.PopupOptions);
    }

    private async void AddFriendsButton_Clicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<string>(
            new InputConnectCodePopup(title: "Input Connect Code"), 
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

        FriendsViewModel.TryAddFriend(result.Result, out _);
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
            FriendsViewModel.TryAddFriend(code, out _);
        }
    }

    private void OnNewActiveGame(FriendViewModel friend, ActiveGameInfo gameInfo)
    {
        if (friend.FriendSettings.NotificationsEnabled &&
            !string.IsNullOrEmpty(gameInfo.PlayerDisplayName) &&
            !string.IsNullOrEmpty(gameInfo.OpponentDisplayName))
        {
            CharacterInfo playerCharacter = CharacterUtils.GetCharacterInfo(gameInfo.PlayerCharacter);
            CharacterInfo opponentCharacter = CharacterUtils.GetCharacterInfo(gameInfo.OpponentCharacter);

            TaskbarIcon.ShowNotification(
                title: $"{friend.FriendSettings.ConnectCode} went live", 
                message: $"{gameInfo.PlayerDisplayName} ({playerCharacter.Name}) vs {gameInfo.OpponentDisplayName} ({opponentCharacter.Name})",
                customIconHandle: TaskbarIcon.Icon!.Handle,
                sound: false,
                largeIcon: true,
                respectQuietTime: true,
                timeout: TimeSpan.FromSeconds(2));
        }
    }

    private void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
    {
        var dragUI = e.PlatformArgs!.DragEventArgs.DragUIOverride;
        dragUI.IsCaptionVisible = false;
        dragUI.IsGlyphVisible = false;
    }
}
