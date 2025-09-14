using SlippiTV.Client.Settings;
using SlippiTV.Shared.Service;
using SlippiTV.Shared.Types;
using System.Diagnostics;
using System.IO.Compression;
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

    public bool ShowProgressBar
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
    } = false;

    public double Progress
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
    } = 0;

    public async Task BeginUpdate(Action<double> onProgress, Action<string> onStatusText)
    {
        onStatusText("Gathering update information...");
        UpdateInfo updateInfo = await Service.GetUpdateInfo();

        string dir = Path.GetDirectoryName(Environment.ProcessPath!)!;

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

        string zipDestination = Path.Join(dir, updateInfo.UpdateFileName);
        HttpClient client = new HttpClient(progressHandler);
        using var response = await client.GetAsync(updateInfo.UpdateLink, CancellationToken.None);

        using (var fileStream = new FileStream(zipDestination, FileMode.Create))
        {
            await response.Content.CopyToAsync(fileStream);
        }

        onStatusText("Extracting update...");
        string zipExtractDir = Path.Join(dir, Path.GetFileNameWithoutExtension(updateInfo.UpdateFileName));
        // now unzip it      
        using (var zipStream = new FileStream(zipDestination, FileMode.Open))
        {
            ZipFile.ExtractToDirectory(zipStream, zipExtractDir, overwriteFiles: true);
        }

        onStatusText("Applying update...");
        string updaterPath = Path.Join(dir, "SlippiTV.Updater.exe");
        if (!File.Exists(updaterPath))
        {
            throw new Exception("Error: SlippiTV.Updater.exe wasn't found. Extract the update manually.");
        }

        await Task.Delay(100);

        // Start the script in a detached process
        var psi = new ProcessStartInfo
        {
            FileName = updaterPath,
            Arguments = $"{Path.GetRelativePath(dir, zipExtractDir)} {Path.GetRelativePath(dir, zipDestination)}",
            WorkingDirectory = dir,
            CreateNoWindow = false,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
        };
        Process.Start(psi);

        Environment.Exit(0);
    }
}
