using SlippiTV.Client.ViewModels;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client;

public partial class BottomBar : ContentView
{
    [NotNull]
    public ShellViewModel? ShellViewModel { get; set; }

	public BottomBar()
	{
		InitializeComponent();

        Loaded += BottomBar_Loaded;
	}

    private void BottomBar_Loaded(object? sender, EventArgs e)
    {
        if (ShellViewModel is not null)
        {
            ShellViewModel.PropertyChanged -= OnShellViewModelPropertyChanged;
        }

        ShellViewModel = (ShellViewModel)this.BindingContext;
        ShellViewModel.PropertyChanged += OnShellViewModelPropertyChanged;
    }

    private void OnShellViewModelPropertyChanged(object? sender, PropertyChangedEventArgs propertyName)
    {
        if (propertyName.PropertyName is null || propertyName.PropertyName == nameof(ShellViewModel.AnimateRelayStatus))
        {
            RelayImage.CancelAnimations();

            if (ShellViewModel.AnimateRelayStatus)
            {
                Animation pulse = new Animation();
                Animation pulseOut = new Animation(v => RelayImage.Scale = v, 1, 1.2, Easing.CubicIn);
                Animation pulseIn = new Animation(v => RelayImage.Scale = v, 1.2, 1, Easing.CubicOut);
                pulse.Add(0, 0.5, pulseOut);
                pulse.Add(0.5, 1, pulseIn);

                RelayImage.Animate("PulseAnimation", pulse, length: 1000, repeat: () => ShellViewModel.AnimateRelayStatus == true);
            }
        }
    }
}