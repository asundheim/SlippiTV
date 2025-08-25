using System.Runtime.InteropServices;

namespace SlippiTV.Client.Platforms.Windows.RustInvoker;

internal static partial class RustInvokes
{
    [LibraryImport("slippi_rust_extensions.dll")]
    internal static partial IntPtr slprs_exi_device_create(SlippiRustEXIConfig config);

    [LibraryImport("slippi_rust_extensions.dll")]
    internal static partial void slprs_exi_device_destroy(IntPtr exiPtr);

    [LibraryImport("slippi_rust_extensions.dll")]
    internal static partial SlippiRustRankInfo slprs_get_rank_info(IntPtr exiPtr);
}
