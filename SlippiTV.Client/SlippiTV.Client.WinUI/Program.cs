using MauiApp = Microsoft.Maui.Controls.Application;
using WinUIApplication = Microsoft.UI.Xaml.Application;
using System.Linq;
using H.NotifyIcon;
using Microsoft.UI.Dispatching;
using Microsoft.Win32;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using SlippiTV.Shared;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;

namespace SlippiTV.Client.WinUI;

public partial class Program
{
    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr CreateEventW(
        IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset,
        [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetEvent(IntPtr hEvent);

    [LibraryImport("ole32.dll")]
    private static partial uint CoWaitForMultipleObjects(
        uint dwFlags, uint dwMilliseconds, ulong nHandles,
        IntPtr[] pHandles, out uint dwIndex);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    private static IntPtr redirectEventHandle = IntPtr.Zero;
    private static App? _app;

    [STAThread]
    public static int Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        AppActivationArguments activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        ExtendedActivationKind kind = activationArgs.Kind;

        // You would think we would get launched with ExtendedActivationKind.Startup, but it's just Launch if we're set from the registry.
        // Unless someone puts our binary into System32 this should be an adequate workaround.
        if (Environment.CurrentDirectory != Environment.GetFolderPath(Environment.SpecialFolder.System))
        {
            TryUpdateLaunchOnStartup(SettingsManager.Instance.Settings.LaunchOnStartup);
        }
        else
        {
            MauiProgram.OpenHidden = true;
        }

        RegisterProtocol();

        AppInstance keyInstance = AppInstance.FindOrRegisterForKey("SlippiTV");
        if (!keyInstance.IsCurrent)
        {
            RedirectActivationTo(activationArgs, keyInstance);
        }
        else
        {
            keyInstance.Activated += OnActivated;
            SettingsManager.Instance.Settings.PropertyChanged += OnSettingsChanged;

            WinUIApplication.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _app = new App();
            });
        }

        return 0;
    }

    private static void TryUpdateLaunchOnStartup(bool launchOnStartup)
    {
        try
        {
            var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
            if (runKey is not null)
            {
                runKey.DeleteValue("SlippiTV", throwOnMissingValue: false);
                if (launchOnStartup)
                {
                    runKey.SetValue("SlippiTV", Environment.ProcessPath!, RegistryValueKind.String);
                }
            }
        }
        catch { }
    }

    private static void TryToggleMenuEvents(bool enableMenuEvents)
    {
        try
        {
            if (Directory.Exists(SettingsManager.Instance.Settings.SlippiLauncherFolder))
            {
                string geckoPath = Path.Join(SettingsManager.Instance.Settings.SlippiLauncherFolder, "netplay", "User", "GameSettings", "GALE01.ini");  
                if (File.Exists(geckoPath))
                {
                    List<string> geckoLines = File.ReadAllLines(geckoPath).ToList();
                    const string menuEventKey = "$Optional: Extract Menu Info";
                    bool menuEventsDefined = false;
                    int geckoStart = -1;
                    int geckoEnd = -1;

                    for (int i = 0; i < geckoLines.Count; i++)
                    {
                        if (geckoLines[i] == "[Gecko]")
                        {
                            geckoStart = i;
                            int geckoExtent = 0;
                            foreach (string line in IterateBlock([..geckoLines.Skip(i)]))
                            {
                                geckoExtent++;
                                if (line.Contains(menuEventKey))
                                {
                                    menuEventsDefined = true;
                                }
                            }

                            geckoEnd = geckoStart + geckoExtent;
                        }
                    }

                    if (!menuEventsDefined && geckoEnd != -1)
                    {
                        geckoLines.InsertRange(geckoEnd, Settings.GeckoCodes.MenuCode.Split(Environment.NewLine));
                    }

                    int geckoEnabledStart = -1;
                    int geckoEnabledExtent = 0;
                    int geckoEnabledKeyLine = -1;
                    for (int i = 0; i < geckoLines.Count; i++)
                    {
                        if (geckoLines[i] == "[Gecko_Enabled]")
                        {
                            geckoEnabledStart = i;
                            foreach (string line in IterateBlock([..geckoLines.Skip(i)]))
                            {
                                geckoEnabledExtent++;
                                if (line.Contains(menuEventKey))
                                {
                                    geckoEnabledKeyLine = geckoEnabledStart + geckoEnabledExtent;
                                    break;
                                }
                            }

                            if (geckoEnabledKeyLine != -1 && !enableMenuEvents)
                            {
                                geckoLines.RemoveAt(geckoEnabledKeyLine);
                            }
                            else if (geckoEnabledKeyLine == -1 && enableMenuEvents)
                            {
                                geckoLines.Insert(geckoEnabledStart + 1, menuEventKey);
                            }
                        }
                    }

                    int geckoDisabledStart = -1;
                    int geckoDisabledExtent = 0;
                    int geckoDisabledKeyLine = -1;
                    for (int i = 0; i < geckoLines.Count; i++)
                    {
                        if (geckoLines[i] == "[Gecko_Disabled]")
                        {
                            geckoDisabledStart = i;
                            foreach (string line in IterateBlock([..geckoLines.Skip(i)]))
                            {
                                geckoDisabledExtent++;
                                if (line.Contains(menuEventKey))
                                {
                                    geckoDisabledKeyLine = geckoDisabledStart + geckoDisabledExtent; 
                                    break;
                                }
                            }

                            if (geckoDisabledKeyLine != -1 && enableMenuEvents)
                            {
                                geckoLines.RemoveAt(geckoDisabledKeyLine);
                            }
                            else if (geckoDisabledKeyLine == -1 && !enableMenuEvents)
                            {
                                geckoLines.Insert(geckoDisabledStart + 1, menuEventKey);
                            }
                        }
                    }

                    File.WriteAllLines(geckoPath, geckoLines);
                }
            }
        }
        catch { }

        static IEnumerable<string> IterateBlock(List<string> blockLines)
        {
            if (blockLines.Count == 0 || !blockLines[0].StartsWith('['))
            {
                yield break;
            }

            foreach (string line in blockLines.Skip(1))
            {
                if (line.StartsWith('['))
                {
                    yield break;
                }

                yield return line;
            }

            yield break;
        }
    }

    private static void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsManager.Settings.LaunchOnStartup))
        {
            TryUpdateLaunchOnStartup(SettingsManager.Instance.Settings.LaunchOnStartup);
        }
        else if (e.PropertyName == nameof(SettingsManager.Settings.EnableMenuEvents))
        {
            TryToggleMenuEvents(SettingsManager.Instance.Settings.EnableMenuEvents);
        }
    }

    private static async void OnActivated(object? sender, AppActivationArguments e)
    {
        if (e.Data is ILaunchActivatedEventArgs launchArgs)
        {
            string[] commandLineArgs = launchArgs.Arguments?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var arg in commandLineArgs)
            {
                var filteredArg = arg.Replace("\"", "");
                if (filteredArg.StartsWith("slippi-tv://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var uri = new Uri(filteredArg);
                        if (uri.Host == "watch")
                        {
                            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                            string? slippiCodeToWatch = query?["code"];
                            if (!string.IsNullOrWhiteSpace(slippiCodeToWatch))
                            {
                                var mainWindow = MauiApp.Current?.Windows.FirstOrDefault();
                                if (mainWindow?.Page is SlippiTV.Client.AppShell appShellInst && 
                                    appShellInst.ShellViewModel?.FriendsViewModel != null)
                                {
                                    await appShellInst.ShellViewModel.FriendsViewModel.WatchByCodeAsync(ConnectCodeUtils.UnsanitizeConnectCode(slippiCodeToWatch));
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        MauiProgram.SlippiTVWindow?.Show(disableEfficiencyMode: true);
    }

    private static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
    {
        redirectEventHandle = CreateEventW(IntPtr.Zero, true, false, null);
        Task.Run(async () =>
        {
            await keyInstance.RedirectActivationToAsync(args);
            SetEvent(redirectEventHandle);
        });

        uint CWMO_DEFAULT = 0;
        uint INFINITE = 0xFFFFFFFF;
        _ = CoWaitForMultipleObjects(
           CWMO_DEFAULT, INFINITE, 1,
           [redirectEventHandle], out uint handleIndex);

        // Bring the window to the foreground
        Process process = Process.GetProcessById((int)keyInstance.ProcessId);
        SetForegroundWindow(process.MainWindowHandle);
    }
    private static void RegisterProtocol()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\slippi-tv");
            key.SetValue("", "URL:SlippiTV Protocol");
            key.SetValue("URL Protocol", "");

            using var shell = key.CreateSubKey("shell");
            using var open = shell.CreateSubKey("open");
            using var command = open.CreateSubKey("command");
            command.SetValue("", $"\"{Environment.ProcessPath}\" \"%1\"");
        }
        catch
        {
        }
    }
}
