using Newtonsoft.Json;
using Slippi.NET.Console.Types;

namespace SlippiTV.Client;

public class SettingsManager
{
    public static SettingsManager Instance = new SettingsManager();

    private readonly string _settingsPath;
    private readonly SlippiTVSettings _settings;

    private SettingsManager()
    {
        string settingsFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlippiTV");
        Directory.CreateDirectory(settingsFolder);

        _settingsPath = Path.Join(settingsFolder, "SlippiTV.settings.json");
        if (File.Exists(_settingsPath) &&
            File.ReadAllText(_settingsPath) is string settingsData &&
            JsonConvert.DeserializeObject<SlippiTVSettings>(settingsData) is SlippiTVSettings existingSettings)
        {
            _settings = existingSettings;
            IsFirstLaunch = false;
        }
        else
        {
            IsFirstLaunch = true;
            _settings = new SlippiTVSettings();

            string defaultPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\Slippi Launcher\playback", "Slippi Dolphin.exe");
            if (File.Exists(defaultPath))
            {
                _settings.WatchDolphinPath = defaultPath;
            }

            SaveSettings();
        }
    }

    public SlippiTVSettings Settings => _settings;
    public bool IsFirstLaunch { get; }

    public ConnectionStatus DolphinConnectionStatus
    {
        get;
        set
        {
            if (field != value)
            {
                OnDolphinConnectionStatus?.Invoke(this, value);
                field = value;
            }
        }
    } = ConnectionStatus.Disconnected;
    public event EventHandler<ConnectionStatus>? OnDolphinConnectionStatus;

    public void AddFriend(string connectCode)
    {
        Settings.Friends.Add(connectCode);
        SaveSettings();
    }

    public void RemoveFriend(string connectCode)
    {
        Settings.Friends.Remove(connectCode);
        SaveSettings();
    }

    public void SaveSettings()
    {
        File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(_settings));
    }
}
