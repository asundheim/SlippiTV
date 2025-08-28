using SlippiTV.Client.Animations;
using SlippiTV.Client.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client;

public partial class SplashScreenPage : ContentPage
{
    [NotNull]
    public ShellViewModel? ShellViewModel { get; set; }

    public SplashScreenPage()
	{
		InitializeComponent();

        Loaded += SplashScreenPage_Loaded;
    }

    private void SplashScreenPage_Loaded(object? sender, EventArgs e)
    {
        SplashScreenImage.Pulse(() => IsLoaded);
        //SplashScreenText.Pulse3(() => IsLoaded);
    }
}