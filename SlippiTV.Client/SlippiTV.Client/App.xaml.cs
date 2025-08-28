using SlippiTV.Client.ViewModels;

namespace SlippiTV.Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            this.SetTheme(SettingsManager.Instance.Settings.Theme);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window()
            {
                Height = 800,
                MinimumHeight = 400,
                MinimumWidth = 600,
                Width = 600,
                Title = "SlippiTV",
            };
            window.Page = new SplashScreenShell(window);

            return window;
        }
    }
}
