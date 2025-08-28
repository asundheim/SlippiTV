using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SlippiTV.Client.Animations;
using SlippiTV.Client.Converters;
using SlippiTV.Client.PlatformUtils;
using SlippiTV.Client.ViewModels;
using SlippiTV.Shared.Types;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ToolTip = Microsoft.UI.Xaml.Controls.ToolTip;

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
        if (propertyName.PropertyName == nameof(ShellViewModel.AnimateRelayStatus))
        {
            if (ShellViewModel.AnimateRelayStatus)
            {
                DolphinStatusIcon.Pulse2(() => ShellViewModel.AnimateRelayStatus == true);
            }
        }
    }
}