using CommunityToolkit.Maui.Extensions;
using SlippiTV.Client.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client;

public partial class SplashScreenShell : Shell
{
    private readonly Window _parent;

    [AllowNull]
    public SplashScreenViewModel SplashScreenViewModel { get; private set; }

	public SplashScreenShell(Window parent)
	{
        _parent = parent;

		InitializeComponent();

        Loaded += SplashScreenShell_Loaded;
	}

    private async void SplashScreenShell_Loaded(object? sender, EventArgs e)
    {
        this.SplashScreenViewModel = (SplashScreenViewModel)this.BindingContext;

        await Task.Delay(500);
        this.SplashScreenViewModel.SplashScreenStatusText = "Checking for updates...";
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
                this.SplashScreenViewModel.ShowProgressBar = true;
                await realShell.SettingsViewModel.BeginUpdate((p) => SplashScreenViewModel.Progress = p, (s) => SplashScreenViewModel.SplashScreenStatusText = s);
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