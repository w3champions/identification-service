using System.Collections.Generic;
using System.Linq;

namespace W3ChampionsIdentificationService.Authorization
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
                "schlüssel#2626" // D2P
            };

        public static bool IsAdmin(string battleTag)
        {
            return ApprovedAdmins.Any(p => p == battleTag.ToLower());
        }
    }
}
