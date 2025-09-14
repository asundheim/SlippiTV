using SlippiTV.Shared.Service;

namespace SlippiTV.Shared.Versions;

public static class ClientVersion
{
    public const string SlippiClientVersion = "0.3.0";

    public static async Task<bool> RequiresUpdateAsync(ISlippiTVService service)
    {
        string newVersion = await service.GetCurrentClientVersion();
        return newVersion != SlippiClientVersion;
    }
}
