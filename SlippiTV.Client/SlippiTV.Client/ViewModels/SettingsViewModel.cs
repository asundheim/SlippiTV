using SlippiTV.Client.Settings;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.Types;
using System.Diagnostics;
using System.Net.Http.Handlers;
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

    public async Task BeginUpdate(Action<double> onProgress, Action<string> onStatusText)
    {
        onStatusText("Gathering update information...");
        UpdateInfo updateInfo = await Service.GetUpdateInfo();

        // Write the script to a temporary file in the current directory
        string dir = Path.GetDirectoryName(Environment.ProcessPath!)!;
        string scriptPath = Path.Combine(dir, "SlippiTV_Update.ps1");
        await File.WriteAllTextAsync(scriptPath, updateInfo.UpdateScript, encoding: Encoding.UTF8);

        // Begin the download
        onStatusText("Downloading update...");
        HttpClientHandler handler = new HttpClientHandler();
        ProgressMessageHandler progressHandler = new ProgressMessageHandler(handler);

        progressHandler.HttpReceiveProgress += (o, args) =>
        {
            if (args.TotalBytes.HasValue && args.TotalBytes.Value != 0)
            {
                onProgress((double)args.BytesTransferred / args.TotalBytes.Value);
            }
        };

        HttpClient client = new HttpClient(progressHandler);
        using var response = await client.GetAsync(updateInfo.UpdateLink, CancellationToken.None);

        using var fileStream = new FileStream(Path.Join(dir, updateInfo.UpdateFileName), FileMode.Create);
        await response.Content.CopyToAsync(fileStream);

        onStatusText("Applying update...");

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
