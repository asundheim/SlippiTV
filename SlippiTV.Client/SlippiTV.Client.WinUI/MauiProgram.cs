using H.NotifyIcon;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Xaml;

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
                                window.AppWindow.Closing += (s, e) =>
                                {
                                    e.Cancel = true;
                                    window.Hide(enableEfficiencyMode: true);
                                };

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
