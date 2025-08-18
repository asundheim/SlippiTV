using H.NotifyIcon;
using Microsoft.Maui.LifecycleEvents;

namespace SlippiTV.Client.WinUI
{
    public static class MauiProgram
    {
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
                            }
                        });
                    });
                })
                .UseSharedMauiApp();

            return builder.Build();
        }
    }
}
