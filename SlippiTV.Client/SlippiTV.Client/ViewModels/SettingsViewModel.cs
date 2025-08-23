namespace SlippiTV.Client.ViewModels;

public class SettingsViewModel : BaseNotifyPropertyChanged
{
    public ShellViewModel ShellViewModel 
    { 
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    public SettingsViewModel(ShellViewModel parent)
    {
        ShellViewModel = parent;
    }

    public string MeleeISOPathLabelText => "Playback Melee .iso path: ";

    public string WatchDolphinPathLabelText => "Playback Dolphin path: ";
}
