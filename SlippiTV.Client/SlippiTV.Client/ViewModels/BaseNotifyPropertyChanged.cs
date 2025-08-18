using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlippiTV.Client.ViewModels;

public class BaseNotifyPropertyChanged : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
