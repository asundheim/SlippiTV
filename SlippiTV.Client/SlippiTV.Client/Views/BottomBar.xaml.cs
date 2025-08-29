using SlippiTV.Client.Animations;
using SlippiTV.Client.ViewModels;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client.Views;

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
        if (propertyName.PropertyName == nameof(ShellViewModel.AnimateRelayStatus))
        {
            if (ShellViewModel.AnimateRelayStatus)
            {
                DolphinStatusIcon.Pulse2(() => ShellViewModel.AnimateRelayStatus == true);
            }
        }
    }
}