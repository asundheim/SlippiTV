using Newtonsoft.Json;
using SlippiTV.Client.ViewModels;

namespace SlippiTV.Client.Settings;

[JsonObject]
public class FriendSettings : BaseNotifyPropertyChanged
{
    public required string ConnectCode { get; set; }
    public bool NotificationsEnabled 
    { 
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SettingsManager.Instance?.SaveSettings();
                OnPropertyChanged();
            }
        }
    } = false;

    public string Nickname
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SettingsManager.Instance?.SaveSettings();
                OnPropertyChanged();
            }
        }
    } = string.Empty;
}
