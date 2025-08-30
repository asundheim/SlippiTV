using CommunityToolkit.Mvvm.Input;

namespace SlippiTV.Client.ViewModels;

public partial class FriendsViewModel
{
    private FriendViewModel? _itemBeingDragged = null;

    // for insertions at the start
    public bool IsBeingDraggedOver { get; set { if (field != value) { field = value; OnPropertyChanged(); } } }

    [RelayCommand]
    public void ItemDragged(FriendViewModel friend)
    {
        friend.IsBeingDragged = true;
        _itemBeingDragged = friend;
    }

    [RelayCommand]
    public void ItemDraggedStart()
    {
        this.IsBeingDraggedOver = true;
    }

    [RelayCommand]
    public void ItemDragLeave(FriendViewModel friend)
    {
        friend.IsBeingDraggedOver = false;
    }

    [RelayCommand]
    public void ItemDragLeaveStart()
    {
        this.IsBeingDraggedOver = false;
    }

    [RelayCommand]
    public void ItemDraggedOver(FriendViewModel friend)
    {
        if (friend == _itemBeingDragged)
        {
            friend.IsBeingDragged = false;
        }

        friend.IsBeingDraggedOver = friend != _itemBeingDragged;
    }

    [RelayCommand]
    public void ItemDroppedStart()
    {
        this.IsBeingDraggedOver = false;

        if (_itemBeingDragged is null)
        {
            return;
        }
        int oldIndex = Friends.IndexOf(_itemBeingDragged);
        if (oldIndex != -1)
        {
            Friends.Move(oldIndex, 0);
            Settings.Friends.Remove(_itemBeingDragged.FriendSettings);
            Settings.Friends.Insert(0, _itemBeingDragged.FriendSettings);
            SettingsManager.Instance.SaveSettings();
        }
    }

    [RelayCommand]
    public void ItemDropped(FriendViewModel friend)
    {
        try
        {
            var itemToMove = _itemBeingDragged;
            var itemToInsertBefore = friend;

            if (itemToMove is null || itemToInsertBefore is null || itemToMove == itemToInsertBefore)
            {
                return;
            }

            int fromIndex = Friends.IndexOf(itemToMove);
            int insertAtIndex = Friends.IndexOf(itemToInsertBefore);
            if (insertAtIndex >= 0 && insertAtIndex < Friends.Count)
            {
                Friends.Remove(itemToMove);
                Settings.Friends.Remove(itemToMove.FriendSettings);
                if (fromIndex > insertAtIndex)
                {
                    Friends.Insert(insertAtIndex + 1, itemToMove);
                    Settings.Friends.Insert(insertAtIndex + 1, itemToMove.FriendSettings);
                }
                else
                {
                    Friends.Insert(insertAtIndex, itemToMove);
                    Settings.Friends.Insert(insertAtIndex, itemToMove.FriendSettings);
                }

                itemToMove.IsBeingDragged = false;
                itemToInsertBefore.IsBeingDraggedOver = false;
                SettingsManager.Instance.SaveSettings();
            }
        }
        catch { }
    }
}
