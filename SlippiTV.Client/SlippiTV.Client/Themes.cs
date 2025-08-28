namespace SlippiTV.Client;

public static class Themes
{
    public const string GCPurple = nameof(GCPurple);
    public const string Light = nameof(Light);
    public const string Dark = nameof(Dark);
}

public static class ThemeUtils
{
    private static readonly List<string> _themedKeys = [
        "Primary",
        "PrimaryDark",
        "PrimaryText",
        "SecondaryText",
        "PrimaryDarkText",
        "Secondary",
        "SecondaryDark",
        "SecondaryDarkText",
        "Tertiary",
        "Quatriary"
    ];

    public static void SetTheme(this Application application, string theme)
    {
        foreach (ResourceDictionary resourceDictionary in application.Resources.MergedDictionaries)
        {
            foreach (var key in _themedKeys)
            {
                if (resourceDictionary.TryGetValue($"{theme}{key}", out var themeValue))
                {
                    resourceDictionary[key] = themeValue;
                }
            }
        }
    }
}
