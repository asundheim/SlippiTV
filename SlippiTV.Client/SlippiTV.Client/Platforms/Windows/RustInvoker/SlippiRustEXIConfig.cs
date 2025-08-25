using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SlippiTV.Client.Platforms.Windows.RustInvoker;

[StructLayout(LayoutKind.Sequential)]
[NativeMarshalling(typeof(SlippiRustEXIConfigMarshaller))]
internal struct SlippiRustEXIConfig
{
    public string IsoPath;
    public string UserConfigFolder;
    public string SlippiSemverString;
    public OsdAddMessageFunction OsdAddMessageFunction;
}

delegate void OsdAddMessageFunction(string msg, uint a, uint b);

[CustomMarshaller(typeof(SlippiRustEXIConfig), MarshalMode.Default, typeof(SlippiRustEXIConfigMarshaller))]
internal unsafe static class SlippiRustEXIConfigMarshaller
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct Native
    {
        public byte* pIsoPath;
        public byte* pUserConfigFolder;
        public byte* pSlippiSemverString;
        public IntPtr pfOsdAddMsgFn;
    }

    public static Native ConvertToUnmanaged(SlippiRustEXIConfig config)
    {
        return new Native
        {
            pIsoPath = Utf8StringMarshaller.ConvertToUnmanaged(config.IsoPath),
            pUserConfigFolder = Utf8StringMarshaller.ConvertToUnmanaged(config.UserConfigFolder),
            pSlippiSemverString = Utf8StringMarshaller.ConvertToUnmanaged(config.SlippiSemverString),
            pfOsdAddMsgFn = Marshal.GetFunctionPointerForDelegate(config.OsdAddMessageFunction)
        };
    }

    public static SlippiRustEXIConfig ConvertToManaged(Native unmanaged)
    {
        return new SlippiRustEXIConfig
        {
            IsoPath = Utf8StringMarshaller.ConvertToManaged(unmanaged.pIsoPath)!,
            UserConfigFolder = Utf8StringMarshaller.ConvertToManaged(unmanaged.pUserConfigFolder)!,
            SlippiSemverString = Utf8StringMarshaller.ConvertToManaged(unmanaged.pSlippiSemverString)!,
            OsdAddMessageFunction = Marshal.GetDelegateForFunctionPointer<OsdAddMessageFunction>(unmanaged.pfOsdAddMsgFn)
        };
    }

    public static void Free(Native unmanaged)
    {
    }
}