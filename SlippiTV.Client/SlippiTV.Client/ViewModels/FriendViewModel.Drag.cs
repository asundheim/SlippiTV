namespace SlippiTV.Client.ViewModels;

public partial class FriendViewModel
{
    public bool IsBeingDraggedOver
    {
        get; set { if (field != value) { field = value; OnPropertyChanged(); } }
    }

    public bool IsBeingDragged
    {
        get; set { if (field != value) { field = value; OnPropertyChanged(); } }
    }
}
