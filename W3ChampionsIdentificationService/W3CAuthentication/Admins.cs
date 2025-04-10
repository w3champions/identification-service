using System.Collections.Generic;
using System.Linq;

namespace W3ChampionsIdentificationService.W3CAuthentication;

public static class Admins
{
    // battletags have to be lowercased!
    private static List<string> ApprovedAdmins =>
        new List<string>
        {
            "gab#1546",
            "floss2xdaily#1987",
            "setcho#2271",
            "askelange#2705",
            "toxi#21818",
            "modmoto#2809",
            "cepheid#1467",
            "helpstone#2919",
            "schlüssel#2626", // D2P
            "fluxxu#1815",
            "gdollaz#1832",
            "martin#2255",
            "curt#1979",
            "kageman#1160",
            "neo#2529",
            "blue#17562", // air
            "marco93f#1535",
            "cloud9#21941", // DS: 272094310927826944 @thecloud9
            "bogdanw3#1673",
            "wontu#1218",
            "bentan#11502", // shorty
            "debaser#1788",
            "fewa418#1895", // 418
            "yolo#24961", // LTD mod, DS: 1135303647253778615 @skarking1
            "brekkie#21685", // Kovax
            "mgeorge#2268", // PlayLikeNeverB4
            "compre#2362",
            "dieseldog#21384",
            "kenshin#2209",
            "fuetti#2605",
            "d4rk#21770",
            "lynx#24118", // D4rk alt account
            "janovi#21973", // SC mod
            "frankenstein#21344", // SC mod
            "voku#2274", // LTD mod, DS: 139191916288933889 @v0ku
            "hodor#2653", // LTD mod, DS: 309432927668928513 @hodor7170
            "artimenius#2503", // SC mod
            "neosc#2938", // LTD mod, DS: 194590058207117312 @mazem1994
            "trillz#1834", // LTD mod, DS: 38824074624897024 @trillz_
            "ipoop4u#1554", // Robbie(Rob)
        };

    public static bool IsAdmin(string battleTag)
    {
        return ApprovedAdmins.Any(p => p == battleTag.ToLower());
    }
}
