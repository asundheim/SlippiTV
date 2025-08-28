using H.NotifyIcon;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SlippiTV.Client.WinUI
{
    public static class MauiProgram
    {
        public static Window? SlippiTVWindow { get; private set; } = null;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .ConfigureLifecycleEvents(events =>
                {
                    events.AddWindows(lifecycleBuilder =>
                    {
                        lifecycleBuilder.OnWindowCreated(window =>
                        {
                            if (window.Title == "SlippiTV")
                            {
                                var appWindow = window.AppWindow;
                                appWindow.Closing += (s, e) =>
                                {
                                    e.Cancel = true;
                                    window.Hide(enableEfficiencyMode: true);
                                };

                                var windowTitleBar = appWindow.TitleBar;
                                var mainColor = (Color.FromArgb("#512BD4").ToWindowsColor());
                                var secColor = (Color.FromArgb("#170457").ToWindowsColor());
                                //var whiteColor = Colors.White.ToWindowsColor();

                                windowTitleBar.BackgroundColor = mainColor;
                                windowTitleBar.ForegroundColor = Colors.White.ToWindowsColor();
                                windowTitleBar.InactiveBackgroundColor = mainColor;
                                windowTitleBar.InactiveForegroundColor = Colors.White.ToWindowsColor();
                                windowTitleBar.ButtonBackgroundColor = mainColor;
                                windowTitleBar.ButtonForegroundColor = Colors.White.ToWindowsColor();
                                windowTitleBar.ButtonInactiveBackgroundColor = mainColor;
                                windowTitleBar.ButtonInactiveForegroundColor = Colors.White.ToWindowsColor();
                                windowTitleBar.ButtonPressedBackgroundColor = secColor;
                                windowTitleBar.ButtonHoverBackgroundColor = secColor;
                                SlippiTVWindow = window;
                            }
                        });
                    });
                })
                .UseSharedMauiApp();

            return builder.Build();
        }
    }
}
