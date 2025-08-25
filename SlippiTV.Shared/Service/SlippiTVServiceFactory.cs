namespace SlippiTV.Shared.Service;

public class SlippiTVServiceFactory
{
    private const bool DEBUG = false;
    public static SlippiTVServiceFactory Instance = new SlippiTVServiceFactory();

    private SlippiTVServiceFactory()
    {
    }

    public ISlippiTVService GetService()
    {
        if (DEBUG)
        {
            return new SlippiTVService("localhost:7027");
        }

        return new SlippiTVService("slippi-tv.azurewebsites.net:443");
    }
}
