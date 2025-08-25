using SlippiTV.Client.Platforms.Windows.RustInvoker;
using System.Runtime.InteropServices;
using static SlippiTV.Client.Platforms.Windows.RustInvoker.RustInvokes;

namespace SlippiTV.Client.Platforms.Windows;

internal partial class DolphinRustInvoker : IDisposable
{
    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetDllDirectoryW(string lpPathName);
    private static bool _initialized = false;

    private IntPtr _exiPtr = IntPtr.Zero;

    private DolphinRustInvoker()
    {
        EnsureInitialized();
    }

    public static Task<DolphinRustInvoker> CreateAsync(string isoPath, string userConfigPath, string versionString)
    {
        DolphinRustInvoker invoker = new DolphinRustInvoker();
        TaskCompletionSource<DolphinRustInvoker> tcs = new TaskCompletionSource<DolphinRustInvoker>();

        _ = Task.Run(async () =>
        {
            invoker._exiPtr = slprs_exi_device_create(new SlippiRustEXIConfig()
            {
                IsoPath = isoPath,
                UserConfigFolder = userConfigPath,
                SlippiSemverString = versionString,
                OsdAddMessageFunction = static (a, b, c) => { },
            });

            // seems like some background threads expect to have some time to set up before giving out accurate info
            await Task.Delay(1000);

            tcs.SetResult(invoker);
        });

        return tcs.Task;
    }

    public SlippiRustRank GetUserRankedInfo()
    {
        SlippiRustRankInfo rankInfo = slprs_get_rank_info(_exiPtr);
        return rankInfo.Rank;
    } 

    private static void EnsureInitialized()
    {
        if (!_initialized)
        {
            SetDllDirectoryW(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Slippi Launcher", "netplay"));
            _initialized = true;
        }
    }

    public void Dispose()
    {
        // this blocks for a looooooong time
        _ = Task.Run(() => slprs_exi_device_destroy(_exiPtr));
    }
}
