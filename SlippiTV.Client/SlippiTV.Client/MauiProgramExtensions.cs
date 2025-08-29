using CommunityToolkit.Maui;
using MauiIcons.Core;
using MauiIcons.Fluent.Filled;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace SlippiTV.Client;

public delegate void EventHandler<TSender, TArgs>(TSender sender, TArgs args);

public static class MauiProgramExtensions
{
    [SupportedOSPlatform("windows10.0.17763")]
    [SupportedOSPlatform("android21.0")]
    [SupportedOSPlatform("ios15.0")]
    [SupportedOSPlatform("maccatalyst15.0")]
    public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .UseFluentFilledMauiIcons()
            .UseMauiIconsCore(x =>
            {
                x.SetDefaultIconSize(24.0);
            })
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldSuppressExceptionsInConverters(true);
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif
        return builder;
    }
}
