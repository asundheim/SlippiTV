using Newtonsoft.Json;
using SlippiTV.Client.ViewModels;
using System.Collections.ObjectModel;

namespace SlippiTV.Client.Settings;

[JsonObject]
public class SlippiTVSettings : BaseNotifyPropertyChanged
{
    public List<FriendSettings> Friends
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged();
            }
        }
    } = [];

    public string StreamMeleeConnectCode 
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowConnectCodeEdit));
            }
        }
    } = string.Empty;

    public string StreamMeleeDisplayName
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged();
            }
        }
    } = string.Empty;

    public string WatchMeleeISOPath 
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged();
            }
        }
    } = string.Empty;

    public string WatchDolphinPath 
    { 
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowWatchDolphinEdit));
            }
        }
    } = string.Empty;

    public string SlippiLauncherFolder 
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged(nameof(ShowConnectCodeEdit));
                OnPropertyChanged(nameof(ShowWatchDolphinEdit));
            }
        }
    } = string.Empty;

    
    public bool ShowConnectCodeEdit => string.IsNullOrEmpty(SlippiLauncherFolder) || string.IsNullOrEmpty(StreamMeleeConnectCode);

    public bool ShowWatchDolphinEdit => string.IsNullOrEmpty(SlippiLauncherFolder) || string.IsNullOrEmpty(WatchDolphinPath);

    public int Version { get; set; } = 3;

    public string Theme
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveSettings();
                OnPropertyChanged();
            }
        }
    } = Themes.Dark;

    public string SlippiVersion { get; set; } = string.Empty;

    public bool LaunchOnStartup 
    { 
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }
    } = true;

    private void SaveSettings()
    {
        SettingsManager.Instance?.SaveSettings();
    }
}
