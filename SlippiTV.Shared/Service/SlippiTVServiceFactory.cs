namespace SlippiTV.Shared.Service;

#pragma warning disable CS0162 // Unreachable code detected

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
