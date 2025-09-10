using CommunityToolkit.Maui.Converters;
using MauiIcons.Fluent.Filled;
using System.Globalization;

namespace SlippiTV.Client.Converters;

public class BoolToNotificationIconConverter : BaseConverterOneWay<bool, FluentFilledIcons>
{
    public override FluentFilledIcons DefaultConvertReturnValue 
    {
        get => FluentFilledIcons.AlertOff24Filled;
        set { }
    }

    public override FluentFilledIcons ConvertFrom(bool value, CultureInfo? culture)
    {
        return value ? FluentFilledIcons.Alert24Filled : FluentFilledIcons.AlertOff24Filled;
    }
}

public class BoolToNotificationColorConverter : BaseConverterOneWay<bool, Color>
{
    public override Color DefaultConvertReturnValue
    {
        get => Color.FromArgb("#333333");
        set { }
    }

    public override Color ConvertFrom(bool value, CultureInfo? culture)
    {
        if (Application.Current is Application application)
        {
            return value ? (Color)Application.Current.Resources["NotificationPrimary"] : (Color)Application.Current.Resources["NotificationSecondary"];
        }

        return value ? Color.FromArgb("#FABF10") : Color.FromArgb("#333333");
    }
}
