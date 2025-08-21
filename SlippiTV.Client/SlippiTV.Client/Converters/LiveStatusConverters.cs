using __XamlGeneratedCode__;
using CommunityToolkit.Maui.Converters;
using SlippiTV.Shared.Service;
using System.Globalization;

namespace SlippiTV.Client.Converters;

public class LiveStatusToColorConverter : BaseConverter<LiveStatus, Color>
{
    public override Color DefaultConvertReturnValue
    {
        get => Colors.Red;
        set { }
    }

    public override LiveStatus DefaultConvertBackReturnValue
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override LiveStatus ConvertBackTo(Color value, CultureInfo? culture) => throw new NotImplementedException();

    public override Color ConvertFrom(LiveStatus value, CultureInfo? culture)
    {
        return value switch
        {
            LiveStatus.Offline => Colors.Red,
            LiveStatus.Active => Colors.Green,
            LiveStatus.Idle => Colors.Yellow,
            _ => Colors.Red
        };
    }
}

public class LiveStatusToToolTipConverter : BaseConverter<LiveStatus, string>
{
    private const string _offline = "Offline";
    private const string _idle = "Idle";
    private const string _online = "Active";

    public override string DefaultConvertReturnValue 
    { 
        get => _offline; 
        set { } 
    }
    public override LiveStatus DefaultConvertBackReturnValue 
    { 
        get => throw new NotImplementedException(); 
        set => throw new NotImplementedException(); 
    }

    public override LiveStatus ConvertBackTo(string value, CultureInfo? culture) => throw new NotImplementedException();

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

