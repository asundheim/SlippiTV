using H.NotifyIcon;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
        AppInstance keyInstance = AppInstance.FindOrRegisterForKey("SlippiTV");

        if (!keyInstance.IsCurrent)
        {
            RedirectActivationTo(activationArgs, keyInstance);
        }
        else
        {
            keyInstance.Activated += OnActivated;

            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _app = new App();
            });
        }

        return 0;
    }

    private static void OnActivated(object? sender, AppActivationArguments e)
    {
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
}
