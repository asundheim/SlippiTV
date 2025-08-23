namespace SlippiTV.Shared.Service;

public class SlippiTVServiceFactory
{
    public static SlippiTVServiceFactory Instance = new SlippiTVServiceFactory();

    private SlippiTVServiceFactory()
    {
    }

    public ISlippiTVService GetService()
    {
        return new SlippiTVService("slippi-tv.azurewebsites.net:443");
    }
}
