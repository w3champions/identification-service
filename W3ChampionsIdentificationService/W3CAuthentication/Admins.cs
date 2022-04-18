using System.Collections.Generic;
using System.Linq;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
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
                "shear#11346",
                "martin#2255",
                "garinthegoat#1294",
                "curt#1979",
                "ipoop4u#1554",
                "ghost#225156",
                "kage#12302",
                "neo#2529",
            };

        public static bool IsAdmin(string battleTag)
        {
            return ApprovedAdmins.Any(p => p == battleTag.ToLower());
        }
    }
}
