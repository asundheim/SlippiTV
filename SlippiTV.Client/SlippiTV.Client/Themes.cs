using SlippiTV.Client.Resources.Styles;

namespace SlippiTV.Client;

public static class Themes
{
    public const string GCPurple = nameof(GCPurple);
    public const string Light = nameof(Light);
    public const string Dark = nameof(Dark);
}

public static class ThemeUtils
{
    public static ResourceDictionary? CurrentTheme;

    public static void SetTheme(this Application application, string theme)
    {
        ResourceDictionary newTheme = theme switch
        {
            Themes.Dark => new DarkTheme(),
            Themes.Light => new LightTheme(),
            Themes.GCPurple => new GCPurpleTheme(),
            _ => new DarkTheme()
        };

        application.Resources = newTheme;
        CurrentTheme = newTheme;
    }
}
