namespace SlippiTV.Client.ViewModels;

public class SplashScreenViewModel : BaseNotifyPropertyChanged
{
    public SplashScreenViewModel()
    {
    }

    public string SplashScreenStatusText
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = "Tuning in...";

    public bool ShowProgressBar
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public double Progress
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }
}
