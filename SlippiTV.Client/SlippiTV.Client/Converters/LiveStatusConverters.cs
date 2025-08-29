using CommunityToolkit.Maui.Converters;
using SlippiTV.Client.ViewModels;
using SlippiTV.Shared.Types;
using System.Globalization;

namespace SlippiTV.Client.Converters;

public class LiveStatusToColorConverter : BaseConverterOneWay<LiveStatus, Color>
{
    public override Color DefaultConvertReturnValue
    {
        get => Color.FromArgb("#DB5151");
        set { }
    }

    public override Color ConvertFrom(LiveStatus value, CultureInfo? culture)
    {
        return value switch
        {
            LiveStatus.Offline => Color.FromArgb("#DB5151"),
            LiveStatus.Active => Color.FromArgb("#10FA94"),
            LiveStatus.Idle => Color.FromArgb("#FABF10"),
            _ => Color.FromArgb("#DB5151")
        };
    }
}

public class LiveStatusToToolTipConverter : BaseConverterOneWay<LiveStatus, string>
{
    private const string _offline = "Offline";
    private const string _idle = "Idle";
    private const string _online = "Active";

    public override string DefaultConvertReturnValue 
    { 
        get => _offline; 
        set { } 
    }

    public override string ConvertFrom(LiveStatus value, CultureInfo? culture)
    {
        return value switch
        {
            LiveStatus.Offline => _offline,
            LiveStatus.Idle => _idle,
            LiveStatus.Active => _online,
            _ => _offline
        };
    }
}

public class LiveStatusToRelayStatusBarToolTipConverter : BaseConverterOneWay<ShellViewModel, string>
{
    private const string _offline = "Disconnected";
    private const string _connecting = "Connecting";
    private const string _connected = "Connected";

    public override string DefaultConvertReturnValue
    {
        get => _offline;
        set { }
    }

    public override string ConvertFrom(ShellViewModel value, CultureInfo? culture)
    {
        return value?.RelayStatus switch
        {
            LiveStatus.Offline => _offline,
            LiveStatus.Idle => _connecting,
            LiveStatus.Active => $"{_connected} to {value.SlippiTVService.SlippiTVServerHost}",
            _ => _offline
        } ?? _offline;
    }
}

public class LiveStatusToDolphinStatusBarToolTipConverter : BaseConverterOneWay<ShellViewModel, string>
{
    private const string _offline = "Disconnected";
    private const string _connecting = "Connecting";
    private const string _connected = "Connected";

    public override string DefaultConvertReturnValue
    {
        get => _offline;
        set { }
    }

    public override string ConvertFrom(ShellViewModel value, CultureInfo? culture)
    {
        return value?.RelayStatus switch
        {
            LiveStatus.Offline => _offline,
            LiveStatus.Idle => _connecting,
            LiveStatus.Active => _connected,
            _ => _offline
        } ?? _offline;
    }
}
