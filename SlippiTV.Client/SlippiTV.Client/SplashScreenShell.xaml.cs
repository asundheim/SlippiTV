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
        var realShell = await ShellViewModel.CreateAsync();
        await Task.Delay(500);
        _parent.Page = new AppShell() { BindingContext = realShell };
    }
}