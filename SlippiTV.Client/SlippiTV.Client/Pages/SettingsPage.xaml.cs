using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using SlippiTV.Client.ViewModels;
using SlippiTV.Client.Views;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client.Pages;

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
        //using DolphinRustInvoker invoker = await DolphinRustInvoker.CreateAsync();

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
            SettingsViewModel.Settings.WatchMeleeISOPath = result.FullPath;
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
            SettingsViewModel.Settings.WatchDolphinPath = result.FullPath;
        }
    }

    private async void EditConnectCodeButton_Clicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<string>(
            new InputConnectCodePopup(title: "Input Your Connect Code"),
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

    private async void UpdateSlippiTV_Clicked(object sender, EventArgs e)
    {
        try
        {
            await SettingsViewModel.BeginUpdate((_) => { }, (_) => { });
        }
        catch
        {
            await this.ShowPopupAsync(new ErrorPopup("Failed to update"));
        }
        
        return;
    }

    private void UpdateThemeDark(object sender, EventArgs e)
    {
        UpdateThemeButtonPressed(Themes.Dark);
    }

    private void UpdateThemeLight(object sender, EventArgs e)
    {
        UpdateThemeButtonPressed(Themes.Light);
    }

    private void UpdateThemeGCPurple(object sender, EventArgs e)
    {
        UpdateThemeButtonPressed(Themes.GCPurple);
    }

    public void UpdateThemeButtonPressed(string themeName)
    {
        if (Application.Current is not null)
        {
            SettingsViewModel.Settings.Theme = themeName;
            Application.Current.SetTheme(themeName);
        }
    }

    private async void RefreshConnectInfo_Clicked(object sender, EventArgs e)
    {
        string folderPath = string.Empty;
        if (System.IO.Path.Exists(SettingsViewModel.Settings.SlippiLauncherFolder))
        {
            folderPath = SettingsViewModel.Settings.SlippiLauncherFolder;
        }
        else if (SettingsManager.Instance.SearchForSlippiLauncher() is string newPath)
        {
            folderPath = newPath;
            SettingsViewModel.Settings.SlippiLauncherFolder = newPath;
        }

        string? errorText = "Could not find Slippi Launcher installation data.";
        if (string.IsNullOrEmpty(folderPath) || !SettingsManager.Instance.UpdateSettingsFromLauncher(folderPath, out errorText))
        {
            await this.ShowPopupAsync(new ErrorPopup(errorText), new PopupOptions()
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(0),
                    StrokeThickness = 0,
                }
            });
        }

        this.SettingsViewModel.ShellViewModel.ReconnectDolphin();
    }
}
