using CommunityToolkit.Maui.Converters;
using MauiIcons.Fluent.Filled;
using SlippiTV.Shared.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        get => Color.FromArgb("#40FFFFFFF");
        set { }
    }

    public override Color ConvertFrom(bool value, CultureInfo? culture)
    {
        return value ? Color.FromArgb("#FABF10") : Color.FromArgb("#ACACAC");
    }
}

public class ActiveGameInfoToIsLocalConverter : BaseConverterOneWay<ActiveGameInfo?, bool>
{
    public override bool DefaultConvertReturnValue 
    {
        get => false;
        set { }
    }

    public bool Invert { get; set; } = false;

    public override bool ConvertFrom(ActiveGameInfo? value, CultureInfo? culture)
    {
        return Invert ? (value?.GameNumber ?? 0) != 0 : (value?.GameNumber ?? 0) == 0;
    }
}
