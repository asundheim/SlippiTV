using Newtonsoft.Json;
using Slippi.NET.Console.Types;
using Slippi.NET.Utils;
using SlippiTV.Client.Settings;
using SlippiTV.Client.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace SlippiTV.Client;

public partial class SettingsManager : BaseNotifyPropertyChanged
{
    public static SettingsManager Instance = new SettingsManager();

    private readonly string _settingsPath;
    private readonly SlippiTVSettings _settings;

    private SettingsManager()
    {
        string settingsFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlippiTV");
        Directory.CreateDirectory(settingsFolder);

        _settingsPath = Path.Join(settingsFolder, "SlippiTV.settings.v8.json");
        if (File.Exists(_settingsPath) &&
            File.ReadAllText(_settingsPath) is string settingsData &&
            JsonConvert.DeserializeObject<SlippiTVSettings>(settingsData) is SlippiTVSettings existingSettings)
        {
            _settings = existingSettings;
            IsFirstLaunch = false;
            ValidateSettings();
        }
        else
        {
            IsFirstLaunch = true;
            _settings = new SlippiTVSettings();

            string? slippiLauncherPath = SearchForSlippiLauncher();
            if (slippiLauncherPath is not null)
            {
                _settings.SlippiLauncherFolder = slippiLauncherPath;
            }

            SaveSettings();
        }
    }

    public SlippiTVSettings Settings => _settings;
    public bool IsFirstLaunch { get; }

    public bool TryCreateFriend(string connectCode, out FriendSettings friendSettings)
    {
        // lazy, could be a dictionary
        if (!Settings.Friends.Any(friend => friend.ConnectCode == connectCode))
        {
            friendSettings = new FriendSettings() { ConnectCode = connectCode };
            Settings.Friends.Add(friendSettings);
            SaveSettings();

            return true;
        }
        else
        {
            friendSettings = Settings.Friends.First(friend => friend.ConnectCode == connectCode);
            return false;
        }
    }

    public bool RemoveFriend(FriendSettings friend)
    {
        bool result = Settings.Friends.Remove(friend);
        SaveSettings();

        return result;
    }

    public void SaveSettings()
    {
        File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(_settings));
    }

    public List<string> AddFromRecentCandidates 
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = [];

    internal string? SearchForSlippiLauncher()
    {
        List<string> candidates = 
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), // see below, this should always be where it is
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) // preemptively guessing where it might move if it did move
        ];

        foreach (string candidate in candidates)
        {
            string slippiLauncherFolder = Path.Combine(candidate, @"Slippi Launcher\");
            if (Directory.Exists(slippiLauncherFolder))
            {
                if (UpdateSettingsFromLauncher(slippiLauncherFolder, out _))
                {
                    return slippiLauncherFolder;
                }
            }
        }

        return null;
    }

    internal bool UpdateSettingsFromLauncher(string launcherPath, [NotNullWhen(false)] out string? errorText)
    {
        errorText = null;

        string? dirName = Path.GetDirectoryName(launcherPath);
        if (dirName is null)
        {
            errorText = "Invalid path";
            return false;
        }

        // it turns out the code in Slippi Launcher *always* writes to AppData
        // https://github.com/project-slippi/slippi-launcher/blob/572c4f94f4c75d5fd94b463920c9464b4ed05aee/src/dolphin/install/mainline_installation.ts#L30
        // so this is unnecessary. if it's not there, we're not gonna find it anywhere else presumably.
        // we'll good-faith show the edit controls in the case we don't find it, assuming they made a breaking change and we're the idiots for relying on it
        if (!dirName.EndsWith("Slippi Launcher"))
        {
            foreach (var directory in Directory.EnumerateDirectories(launcherPath, "Slippi Launcher", new EnumerationOptions() { RecurseSubdirectories = true, MaxRecursionDepth = 10 }))
            {
                bool recurseResult = UpdateSettingsFromLauncher(directory, out errorText);
                if (recurseResult)
                {
                    return true;
                }
            }

            errorText ??= "Could not find directory \"Slippi Launcher\" under provided path";
            return false;
        }

        string userJsonPath = Path.Combine(launcherPath, @"netplay\User\Slippi\user.json");
        string? connectCodeTemp = null;
        string? displayNameTemp = null;
        string? latestVersionTemp = null;
        if (File.Exists(userJsonPath))
        {
            var userJsonType = new 
            {
                connectCode = "", 
                displayName = "",
                latestVersion = ""
            };
            var result = JsonConvert.DeserializeAnonymousType(File.ReadAllText(userJsonPath), userJsonType, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
            if (result is not null)
            {
                connectCodeTemp = result.connectCode;
                displayNameTemp = result.displayName;
                latestVersionTemp = result.latestVersion;
            }
            else
            {
                connectCodeTemp = string.Empty;
            }
        }

        string directCodesPath = Path.Combine(launcherPath, @"netplay\User\Slippi\direct-codes.json");
        List<string>? addFromRecentCandidates = null;
        // TODO do we actually want this?
        //if (IsFirstLaunch && File.Exists(directCodesPath))
        //{
        //    var directCodesType = new { connectCode = "" };
        //    var result = JsonConvert.DeserializeAnonymousType(File.ReadAllText(directCodesPath), Enumerable.Range(0, 0).Select(x => directCodesType).ToList());
        //    if (result is not null)
        //    {
        //        foreach (var candidate in result)
        //        {
        //            string normalized = FullWidthConverter.ToHalfwidth(candidate.connectCode);
        //            if (_settings.Friends.Any(friend => friend.ConnectCode == normalized))
        //            {
        //                continue;
        //            }

        //            addFromRecentCandidates ??= new List<string>();
        //            addFromRecentCandidates.Add(normalized);
        //        }
        //    }
        //}

        string playbackDolphinPath = Path.Join(launcherPath, @"playback", "Slippi Dolphin.exe");
        string? playbackPathTemp = null;
        if (Path.Exists(playbackDolphinPath))
        {
            playbackPathTemp = playbackDolphinPath;
        }

        if (playbackPathTemp is not null && connectCodeTemp is not null)
        {
            // always set these - the file should be the source of truth
            _settings.StreamMeleeConnectCode = connectCodeTemp;
            _settings.SlippiVersion = latestVersionTemp ?? string.Empty;

            // set this if it's empty
            if (string.IsNullOrEmpty(_settings.WatchDolphinPath))
            {
                _settings.WatchDolphinPath = playbackPathTemp;
            }
            
            //  set this if we found it
            if (displayNameTemp is not null)
            {
                _settings.StreamMeleeDisplayName = displayNameTemp;
            }

            // set these if we found them
            if (addFromRecentCandidates is not null)
            {
                // this.AddFromRecentCandidates = addFromRecentCandidates;
                foreach (var recentFriend in addFromRecentCandidates.Take(10))
                {
                    TryCreateFriend(recentFriend, out _);
                }
            }

            // set this if it's empty
            if (string.IsNullOrEmpty(_settings.WatchMeleeISOPath))
            {
                string? isoPath = FindMeleeIsoFromLauncher(launcherPath);
                if (isoPath is not null)
                {
                    _settings.WatchMeleeISOPath = isoPath;
                }
            }

            return true;
        }

        if (playbackPathTemp is null)
        {
            errorText = $"Unable to find \"Slippi Dolphin.exe\" under the path {playbackDolphinPath}";
        }
        else
        {
            errorText = $"Unable to find \"user.json\" under the path {userJsonPath}";
        }

        return false;
    }

    [GeneratedRegex(@"^LastFilename = (.*)", RegexOptions.Compiled)]
    private static partial Regex _dolphinIniIsoRegex();

    private static string? FindMeleeIsoFromLauncher(string launcherPath)
    {
        string settingsPath = Path.Combine(launcherPath, "Settings");
        if (File.Exists(settingsPath))
        {
            var settingsType = new { settings = new { isoPath = "" } };
            var result = JsonConvert.DeserializeAnonymousType(File.ReadAllText(settingsPath), settingsType, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
            if (result is not null && result.settings is not null && File.Exists(result.settings.isoPath))
            {
                return result.settings.isoPath;
            }
        }

        string dolphinIni = Path.Combine(launcherPath, @"playback\User\Config\Dolphin.ini");
        if (File.Exists(dolphinIni))
        {
            string[] iniContents = File.ReadAllLines(dolphinIni);
            foreach (var line in iniContents)
            {
                var matchResult = _dolphinIniIsoRegex().Match(line);
                if (matchResult.Success && File.Exists(matchResult.Groups[1].Value))
                {
                    return matchResult.Groups[1].Value.Trim();
                }
            }
        }

        return null;
    }

    private void ValidateSettings()
    {
        // clear out invalid settings (files moved)
        if (string.IsNullOrEmpty(_settings.WatchDolphinPath) || !Path.Exists(_settings.WatchDolphinPath))
        {
            _settings.WatchDolphinPath = string.Empty;
        }

        if (string.IsNullOrEmpty(_settings.WatchMeleeISOPath) || !Path.Exists(_settings.WatchMeleeISOPath))
        {
            _settings.WatchMeleeISOPath = string.Empty;
        }

        // recheck the launcher settings. if we failed to parse them, re-search and try to set them. if that fails then give up
        if (string.IsNullOrEmpty(_settings.SlippiLauncherFolder) || !UpdateSettingsFromLauncher(_settings.SlippiLauncherFolder, out _))
        {
            string? searchResult = SearchForSlippiLauncher();
            if (string.IsNullOrEmpty(searchResult))
            {
                _settings.SlippiLauncherFolder = string.Empty;
            }
            else
            {
                _settings.SlippiLauncherFolder = searchResult;
            }
        }
    }
}
