using SlippiTV.Client.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace SlippiTV.Client;

public partial class AppShell : Shell
{
    [AllowNull]
    public ShellViewModel ShellViewModel { get; set; }

    public AppShell()
    {
        InitializeComponent();
        Loaded += AppShell_Loaded;
    }

    private void AppShell_Loaded(object? sender, EventArgs e)
    {
        ShellViewModel = (ShellViewModel)this.BindingContext;
    }
}
