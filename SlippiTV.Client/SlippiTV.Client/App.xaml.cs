using SlippiTV.Client.ViewModels;

namespace SlippiTV.Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell() { BindingContext = new ShellViewModel() })
            {
                Height = 800,
                MinimumHeight = 300,
                MinimumWidth = 500,
                Width = 500,
                Title = "SlippiTV"
            };

            return window;
        }
    }
}
