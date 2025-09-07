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

    public async Task BeginUpdate(Action<double> onProgress, Action<string> onStatusText)
    {
        onStatusText("Gathering update information...");
        UpdateInfo updateInfo = await Service.GetUpdateInfo();

        // Write the script to a temporary file in the current directory
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

        string scriptPath = Path.Join(dir, "SlippiTVUpdate.bat");
        string swapScript =
        $$"""
        @echo off
        setlocal enabledelayedexpansion

        echo ====================================================
        echo   ____    _   _                   _   _______     __
        echo  / ___^|  ^| ^| (_)  _ __    _ __   (_) ^|_   _\ \   / /
        echo  \___ \  ^| ^| ^| ^| ^|  _ \  ^|  _ \  ^| ^|   ^| ^|  \ \ / / 
        echo   ___) ^| ^| ^| ^| ^| ^| ^|_) ^| ^| ^|_) ^| ^| ^|   ^| ^|   \ V /  
        echo  ^|____/  ^|_^| ^|_^| ^| .__/  ^| .__/  ^|_^|   ^|_^|    \_/   
        echo                  ^|_^|     ^|_^|                        
        echo ====================================================

        REM Verify SlippiTV.exe exists
        if not exist "SlippiTV.exe" (
            echo [ERROR] SlippiTV.exe not found in the current directory.
            echo Please ensure this script is in the same folder as SlippiTV.exe.
            pause
            exit /b 1
        )

        REM Kill all active instances of SlippiTV.exe
        echo [INFO] Checking for running instances of SlippiTV.exe...
        tasklist /fi "ImageName eq SlippiTV.exe" | find /I "SlippiTV.exe" >nul 2>&1
        if not errorlevel 1 (
            echo [INFO] Killing active instances of SlippiTV.exe...
            taskkill /f /im "SlippiTV.exe" >nul 2>&1
            if errorlevel 1 (
                echo [ERROR] Failed to terminate SlippiTV.exe. Please close it manually and try again.
                pause
                exit /b 1
            )
            echo [INFO] All instances of SlippiTV.exe terminated.
        ) else (
            echo [INFO] No active instances of SlippiTV.exe found.
        )

        set "tempFile={{zipExtractDir}}\SlippiTV.exe"
        REM Replace the old .exe with the new .exe
        echo [INFO] Replacing the old SlippiTV.exe with the new version...
        del "SlippiTV.exe" >nul 2>&1
        if errorlevel 1 (
            echo [ERROR] Failed to delete the old SlippiTV.exe. Please check file permissions.
            pause
            exit /b 1
        )
        move /y "!tempFile!" "SlippiTV.exe" >nul 2>&1
        if errorlevel 1 (
            echo [ERROR] Failed to replace SlippiTV.exe. Please check file permissions.
            pause
            exit /b 1
        )
        echo [INFO] Replacement successful.

        REM Delete the zip 
        del "{{zipDestination}}" >nul 2>&1
        if errorlevel 1 (
            echo [WARNING] Could not delete ZIP file. You may remove it manually.
        ) else (
            echo [INFO] ZIP file removed.
        )

        REM Delete the extracted directory
        rmdir /s /q "{{zipExtractDir}}" >nul 2>&1
        if errorlevel 1 (
            echo [WARNING] Could not delete extracted directory. You may remove it manually.
        ) else (
            echo [INFO] Extracted directory removed.
        )

        REM Relaunch SlippiTV.exe
        echo [INFO] Relaunching SlippiTV.exe...
        start "" "SlippiTV.exe"
        if errorlevel 1 (
            echo [ERROR] Failed to relaunch SlippiTV.exe. Please start it manually.
            pause
            exit /b 1
        )
        echo [INFO] SlippiTV.exe relaunched successfully.
        start /B cmd.exe /C "ping 127.0.0.1 -n 2 > nul && del "{{scriptPath}}"
        exit 0
        """;

        onStatusText("Applying update...");
        
        File.WriteAllText(scriptPath, swapScript, Encoding.ASCII);

        await Task.Delay(100);

        // Start the script in a detached process
        var psi = new ProcessStartInfo
        {
            FileName = $"{scriptPath}",
            WorkingDirectory = dir,
            CreateNoWindow = false,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
        };
        Process.Start(psi);
    }
}
