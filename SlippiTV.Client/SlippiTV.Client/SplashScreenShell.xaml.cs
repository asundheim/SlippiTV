using CommunityToolkit.Maui.Extensions;
using SlippiTV.Client.ViewModels;

namespace SlippiTV.Client;

public partial class SplashScreenShell : Shell
{
    private Window _parent;
	public SplashScreenShell(Window parent)
	{
        _parent = parent;

		InitializeComponent();

        Loaded += SplashScreenShell_Loaded;
	}

    private async void SplashScreenShell_Loaded(object? sender, EventArgs e)
    {
        await Task.Delay(1000);
        try
        {
            var realShell = await ShellViewModel.CreateAsync();
            if (!realShell.RequiresUpdate)
            {
                await Task.Delay(500);
                _parent.Page = new AppShell() { BindingContext = realShell };
            }
            else
            {
                await this.ShowPopupAsync(new ErrorPopup("An update is available and will begin downloading."));
                await realShell.SettingsViewModel.BeginUpdate();
                Environment.Exit(0);
            }
        }
        catch (HttpRequestException)
        {
            await this.ShowPopupAsync(new ErrorPopup("Failed to contact SlippiTV server"));
            Environment.Exit(0);
        }
    }
}