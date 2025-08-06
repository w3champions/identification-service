using System;
using System.Collections.Generic;
using System.Linq;

namespace W3ChampionsIdentificationService.Blizzard;

public enum BlizzardPlayableTitle
{
    BlizzardArcadeCollection = 1381257807,
    CallOfDutyHQ = 1096108883,
    CallOfDutyBlackOps4 = 1447645266,
    CallOfDutyBlackOps4BlackOpsPass = 1112486992,
    CallOfDutyBlackOps4ClassifiedZombiesExperience = 1112487002,
    CallOfDutyBlackOpsColdWar = 1514493267,
    CallOfDutyModernWarfare = 1329875278,
    CallOfDutyModernWarfareII = 1297566025,
    CallOfDutyModernWarfareIII = 5068595,
    CallOfDutyMW2Remastered = 1279351378,
    CallOfDutyVanguard = 1179603525,
    CrashBandicoot4 = 1464615513,
    Diablo = 1146246220,
    Diablo2Resurrected = 5198665,
    DiabloImmortal = 1095647827,
    DiabloIV = 4613486,
    Diablo2 = 1144144982,
    Diablo2Expansion = 1144150096,
    Diablo3 = 17459,
    Diablo3ReaperOfSouls = 1144215632,
    Hearthstone = 1465140039,
    HeroesOfTheStorm = 1214607983,
    Overwatch2 = 5272175,
    StarCraft = 1398030674,
    StarCraftRemastered = 21297,
    StarCraft2 = 21298,
    StarCraft2WingsOfLiberty = 1395804243,
    StarCraft2HeartOfTheSwarm = 1395805270,
    StarCraft2LegacyOfTheVoid = 1395808076,
    WarcraftOrcsAndHumans = 1463898673,
    Warcraft2 = 1462911566,
    Warcraft3ReignOfChaos = 1463898675,
    Warcraft3FrozenThrone = 1462982736,
    Warcraft3Reforged = 22323,
    WarcraftRumble = 4674137,
    WorldOfWarcraft = 5730135,
    WorldOfWarcraftDragonflight = 1464817475,
    WorldOfWarcraftClassicAndExpansions = 1465397552,
}

public static class BlizzardPlayableTitleExtensions
{
    public static List<BlizzardPlayableTitle> FromTitleIds(int[] titleIds)
    {
        return (titleIds ?? Array.Empty<int>())
            .Where(titleId => Enum.IsDefined(typeof(BlizzardPlayableTitle), titleId))
            .Select(titleId => (BlizzardPlayableTitle)titleId)
            .ToList();
    }
}
