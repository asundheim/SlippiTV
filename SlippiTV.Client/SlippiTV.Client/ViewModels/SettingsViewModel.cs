using SlippiTV.Shared.Service;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SlippiTV.Client.ViewModels;

public class SettingsViewModel : BaseNotifyPropertyChanged
{
    public ShellViewModel ShellViewModel 
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
    }

    public SlippiTVSettings Settings => SettingsManager.Instance.Settings;

    public ISlippiTVService Service => ShellViewModel.SlippiTVService;

    public SettingsViewModel(ShellViewModel parent)
    {
        ShellViewModel = parent;
    }

    public string MeleeISOPathLabelText => "Playback Melee .iso path: ";

    public string WatchDolphinPathLabelText => "Playback Dolphin path: ";

    public async Task BeginUpdate()
    {
        string scriptContents = await Service.GetUpdateScript();

        // Write the script to a temporary .cmd file in the current directory
        string dir = Path.GetDirectoryName(Environment.ProcessPath!)!;
        string scriptPath = Path.Combine(dir, "SlippiTV_Update.ps1");
        await File.WriteAllTextAsync(scriptPath, scriptContents, encoding: Encoding.UTF8);

        // Start the script in a detached process
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"\"{scriptPath}\"",
            WorkingDirectory = dir,
            CreateNoWindow = false,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
        };
        Process.Start(psi);
    }
}
