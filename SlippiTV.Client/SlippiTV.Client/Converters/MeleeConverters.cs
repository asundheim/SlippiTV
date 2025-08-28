using CommunityToolkit.Maui.Converters;
using Newtonsoft.Json.Linq;
using Slippi.NET.Melee;
using Slippi.NET.Melee.Data;
using Slippi.NET.Melee.Types;
using SlippiTV.Shared.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlippiTV.Client.Converters;

public class StageToImageConverter : BaseConverterOneWay<Stage, ImageSource>
{
    public override ImageSource DefaultConvertReturnValue 
    {
        get => string.Empty;
        set { }
    }

    public override ImageSource ConvertFrom(Stage stage, CultureInfo? culture)
    {
        return stage switch
        {
            Stage.Battlefield => "map-bf.png",
            Stage.Dreamland => "map-dl.png",
            Stage.FinalDestination => "map-fd.png",
            Stage.FountainOfDreams => "map-fod.png",
            Stage.PokemonStadium => "map-pkm.png",
            Stage.YoshisStory => "map-ys.png",
            _ => string.Empty
        };
    }
}

public class CharacterToImageSourceConverter : BaseConverterOneWay<ActiveGameInfo?, ImageSource>
{
    public override ImageSource DefaultConvertReturnValue
    {
        get => string.Empty;
        set { }
    }

    public bool IsOpponent { get; set; } = false;

    public override ImageSource ConvertFrom(ActiveGameInfo? gameInfo, CultureInfo? culture)
    {
        if (gameInfo is null)
        {
            return string.Empty;
        }

        CharacterInfo characterInfo = CharacterUtils.GetCharacterInfo(IsOpponent ? gameInfo.OpponentCharacter : gameInfo.PlayerCharacter);

        var name = characterInfo.Id;
        var color = CharacterUtils.GetCharacterColorName(name, IsOpponent ? gameInfo.OpponentCharacterColor : gameInfo.PlayerCharacterColor);

        return $"si_{name}_{color}.png";
    }
}

//public class StocksToIntConverter : BaseConverterOneWay<PlayerStocksLeft, int>
//{
//    public override int DefaultConvertReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//    public override int ConvertFrom(PlayerStocksLeft value, CultureInfo? culture)
//    {
//        throw new NotImplementedException();
//    }
//}


