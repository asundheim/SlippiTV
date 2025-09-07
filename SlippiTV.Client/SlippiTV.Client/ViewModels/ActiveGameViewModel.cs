using Slippi.NET.Melee.Types;
using Slippi.NET.Types;
using SlippiTV.Shared.Types;
using System.Collections.ObjectModel;

namespace SlippiTV.Client.ViewModels;

public class ActiveGameViewModel : BaseNotifyPropertyChanged
{
    public FriendViewModel Parent {  get; set; }

    public ActiveGameViewModel(FriendViewModel parent)
    {
        IsActive = false;
        Parent = parent;
    }

    public bool IsActive
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

    public bool IsNetplay
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

    // this all kinda sucks but it makes the eventing real easy
    public string? PlayerConnectCode
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

    public string? PlayerDisplayName
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

    public Character PlayerCharacter
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

    public byte PlayerCharacterColor
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

    public ObservableCollection<int> PlayerStocksLeft 
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
    } = new ObservableCollection<int>();

    public string? OpponentConnectCode
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

    public string? OpponentDisplayName
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

    public Character OpponentCharacter
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

    public byte OpponentCharacterColor
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

    public ObservableCollection<int> OpponentStocksLeft 
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
    } = new ObservableCollection<int>();

    public int GameNumber
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNetplay));
            }
        }
    }

    public Stage Stage
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

    /// <summary>
    /// Rather than update the entire object and cause the screen to flicker, we can just update the re-exposed properties
    /// and rely on property changed logic.
    /// </summary>
    public void UpdateGameInfo(ActiveGameInfo? newGameInfo)
    {
        if (newGameInfo is null)
        {
            IsActive = false;
            ResetAllProperties();
        }
        else
        {
            IsActive = true;
            Stage = newGameInfo.Stage;
            GameNumber = newGameInfo.GameNumber;
            IsNetplay = newGameInfo.GameMode == GameMode.ONLINE;

            if (PlayerCharacter != newGameInfo.PlayerCharacter)
            {
                PlayerStocksLeft = new ObservableCollection<int>();
            }

            if (OpponentCharacter != newGameInfo.OpponentCharacter)
            {
                OpponentStocksLeft = new ObservableCollection<int>();
            }

            OpponentCharacterColor = newGameInfo.OpponentCharacterColor;
            PlayerCharacterColor = newGameInfo.PlayerCharacterColor;
            OpponentCharacter = newGameInfo.OpponentCharacter;
            PlayerCharacter = newGameInfo.PlayerCharacter;
            OpponentConnectCode = newGameInfo.OpponentConnectCode;
            PlayerConnectCode = newGameInfo.PlayerConnectCode;
            OpponentDisplayName = newGameInfo.OpponentDisplayName;
            PlayerDisplayName = newGameInfo.PlayerDisplayName;

            // it's not smart but it works to template out N stock icons
            int originalPlayerCount = PlayerStocksLeft.Count;
            if (originalPlayerCount < newGameInfo.PlayerStocksLeft)
            {
                PlayerStocksLeft = new ObservableCollection<int>();
                for (int i = 0; i < newGameInfo.PlayerStocksLeft; i++)
                {
                    PlayerStocksLeft.Add(i);
                }
            }
            else if (PlayerStocksLeft.Count > newGameInfo.PlayerStocksLeft)
            {
                for (int i = 0; i < originalPlayerCount- newGameInfo.PlayerStocksLeft; i++)
                {
                    PlayerStocksLeft.RemoveAt(PlayerStocksLeft.Count - 1);
                }
            }

            int originalOpponentCount = OpponentStocksLeft.Count;
            if (originalOpponentCount < newGameInfo.OpponentStocksLeft)
            {
                OpponentStocksLeft = new ObservableCollection<int>();
                for (int i = 0; i < newGameInfo.OpponentStocksLeft; i++)
                {
                    OpponentStocksLeft.Add(i);
                }
            }  
            else if (OpponentStocksLeft.Count > newGameInfo.OpponentStocksLeft)
            {
                for (int i = 0; i < originalOpponentCount - newGameInfo.OpponentStocksLeft; i++)
                {
                    OpponentStocksLeft.RemoveAt(OpponentStocksLeft.Count - 1);
                }
            }
        }
    }

    private void ResetAllProperties()
    {
        Stage = default;
        GameNumber = default;
        IsNetplay = default;
        OpponentStocksLeft = new ObservableCollection<int>();
        PlayerStocksLeft = new ObservableCollection<int>();
        OpponentCharacter = default;
        PlayerCharacter = default;
        PlayerCharacterColor = default;
        OpponentCharacterColor = default;
        OpponentConnectCode = default;
        PlayerConnectCode = default;
        OpponentDisplayName = default;
        PlayerDisplayName = default;
    }
}
