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

    public string MeleeISOPathLabelText => $"Melee .iso path: ";
    public string WatchDolphinLabelText => $"Playback Dolphin path: ";

    public void UpdateMeleeISOPath(string meleeISOPath)
    {
        Settings.WatchMeleeISOPath = meleeISOPath;
    }

    public void UpdateWatchDolphinPath(string watchDolphinPath)
    {
        Settings.WatchDolphinPath = watchDolphinPath;
    }
}
