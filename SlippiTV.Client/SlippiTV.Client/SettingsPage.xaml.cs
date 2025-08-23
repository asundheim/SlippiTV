using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using SlippiTV.Client.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client;

public partial class SettingsPage : ContentPage
{
    [NotNull]
	public SettingsViewModel? SettingsViewModel 
    { 
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

	public SettingsPage()
	{
		InitializeComponent();
        Loaded += SettingsPage_Loaded;
	}

    private void SettingsPage_Loaded(object? sender, EventArgs e)
    {
        SettingsViewModel = (SettingsViewModel)this.BindingContext;
    }

    private async void MeleeIsoBrowseButton_Clicked(object sender, EventArgs e)
    {
        PickOptions options = new PickOptions()
        {
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".iso"]
            }),
            PickerTitle = "Select SSBM .iso file"
        };

        var result = await FilePicker.PickAsync(options);
        if (result is not null)
        {
            SettingsViewModel.UpdateMeleeISOPath(result.FullPath);
        }
    }

    private async void WatchDolphinBrowseButton_Clicked(object sender, EventArgs e)
    {
        PickOptions options = new PickOptions()
        {
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".exe"]
            }),
            PickerTitle = "Select Slippi Dolphin Playback .exe"
        };

        var result = await FilePicker.PickAsync(options);
        if (result is not null)
        {
            SettingsViewModel.UpdateMeleeISOPath(result.FullPath);
        }
    }

    private async void EditConnectCodeButton_Clicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<string>(
            new InputTextPopup(
                placeholderText: "Enter Connect Code (ABC#123)",
                title: "Input Your Connect Code"
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

        SettingsManager.Instance.Settings.StreamMeleeConnectCode = result.Result;
    }
}
