using Refit;

namespace SlippiTV.Shared.Service;

public class SlippiTVServiceFactory
{
    public static SlippiTVServiceFactory Instance = new SlippiTVServiceFactory();

    private SlippiTVServiceFactory()
    {
    }

    public ISlippiTVService GetService()
    {
        return RestService.For<ISlippiTVService>("https://localhost:7027");
    }
}
