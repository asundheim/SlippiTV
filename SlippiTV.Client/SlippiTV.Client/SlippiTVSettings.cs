using Newtonsoft.Json;
using SlippiTV.Client.ViewModels;
using System.Collections.ObjectModel;

namespace SlippiTV.Client;

[JsonObject]
public class SlippiTVSettings : BaseNotifyPropertyChanged
{
    public ObservableCollection<string> Friends 
    { 
        get; 
        set
        {
            field = value;
            SaveSettings();
            OnPropertyChanged();
        }
    } = [];

    public string StreamMeleeConnectCode 
    { 
        get; 
        set
        {
            field = value;
            SaveSettings();
            OnPropertyChanged();
        }
    } = string.Empty;

    public string WatchMeleeISOPath 
    {
        get;
        set
        {
            field = value;
            SaveSettings();
            OnPropertyChanged();
        }
    } = string.Empty;

    public string WatchDolphinPath 
    { 
        get;
        set
        {
            field = value;
            SaveSettings();
            OnPropertyChanged();
        }
    } = string.Empty;

    private void SaveSettings()
    {
        SettingsManager.Instance?.SaveSettings();
    }
}
