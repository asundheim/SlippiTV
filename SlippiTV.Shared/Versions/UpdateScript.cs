namespace SlippiTV.Shared.Versions;

public static class UpdateScript
{
    public const string UpdateZipName = @"SlippiTV.0.0.5-beta.zip";
    public const string UpdateDownloadLink = @$"https://github.com/asundheim/SlippiTV/releases/download/0.0.5-beta/{UpdateZipName}";

    public const string Powershell =
    $$"""
    $Host.UI.RawUI.BackgroundColor = "Black"
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host "===================================================="
    Write-Host "  ____    _   _                   _   _______     __"
    Write-Host " / ___|  | | (_)  _ __    _ __   (_) |_   _\ \   / /"
    Write-Host " \___ \  | | | | | '_ \  | '_ \  | |   | |  \ \ / / "
    Write-Host "  ___) | | | | | | |_) | | |_) | | |   | |   \ V /  "
    Write-Host " |____/  |_| |_| | .__/  | .__/  |_|   |_|    \_/   "
    Write-Host "                 |_|     |_|                        "
    Write-Host "===================================================="
    Write-Host ""
    Write-Host "SlippiTV Updater"
    Write-Host ""

    $exe = "SlippiTV.exe"
    $zip = "{{UpdateZipName}}"
    $scriptPath = $MyInvocation.MyCommand.Path

    function Delete-Self-And-Exit
    {
        Start-Process cmd.exe -ArgumentList "/C ping 127.0.0.1 -n 2 > nul & del `"$scriptPath`"" -WindowStyle Hidden
        exit 0
    }

    # Verify SlippiTV.exe exists
    if (-not (Test-Path -Path $exe)) {
        Write-Host "[ERROR] SlippiTV.exe not found in the current directory: $scriptPath"
        Write-Host "Please ensure this script is in the same folder as SlippiTV.exe."
        Read-Host "Press Enter to exit"
        Delete-Self-And-Exit
    }

    # Verify zip exists
    if (-not (Test-Path -Path $zip))
    {
        Write-Host "[ERROR] $zip not found in the current directory: $scriptPath"
        Write-Host "Please ensure this script is in the same folder as $zip"
        Read-Host "Press Enter to exit"
        Delete-Self-And-Exit
    }

    # Kill all active instances of SlippiTV.exe
    Write-Host "[INFO] Checking for running instances of SlippiTV.exe..."
    $procs = Get-Process | Where-Object { $_.ProcessName -eq "SlippiTV" }
    if ($procs) {
        Write-Host "[INFO] Killing active instances of SlippiTV.exe..."
        try {
            $procs | Stop-Process -Force -ErrorAction Stop
            Write-Host "[INFO] All instances of SlippiTV.exe terminated."
        } catch {
            Write-Host "[ERROR] Failed to terminate SlippiTV.exe. Please close it manually and try again."
            Read-Host "Press Enter to exit"
            Delete-Self-And-Exit
        }
    } else {
        Write-Host "[INFO] No active instances of SlippiTV.exe found."
    }

    # Unzip the new version (overwrite existing files)
    Write-Host "[INFO] Extracting update..."
    try {
        Expand-Archive -Path $zip -DestinationPath "." -Force
        Write-Host "[INFO] Extraction completed."
    } catch {
        Write-Host "[ERROR] Failed to extract the update ZIP. Please check file permissions."
        Read-Host "Press Enter to exit"
        Delete-Self-And-Exit
    }

    # Clean up the ZIP file
    Write-Host "[INFO] Cleaning up..."
    try {
        Remove-Item $zip -Force -ErrorAction Stop
        Write-Host "[INFO] ZIP file removed."
    } catch {
        Write-Host "[WARNING] Could not delete ZIP file. You may remove it manually."
    }

    # Relaunch SlippiTV.exe
    Write-Host "[INFO] Relaunching SlippiTV.exe..."
    try {
        Start-Process ".\SlippiTV.exe"
        Write-Host "[INFO] SlippiTV.exe relaunched successfully."
    } catch {
        Write-Host "[ERROR] Failed to relaunch SlippiTV.exe. Please start it manually."
        Read-Host "Press Enter to exit"
        Delete-Self-And-Exit
    }

    Write-Host ""
    Write-Host "[INFO] Update process completed successfully."
    Delete-Self-And-Exit
    """;
}
