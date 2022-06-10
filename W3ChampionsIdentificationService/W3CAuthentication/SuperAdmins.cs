using System.Collections.Generic;
using System.Linq;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public static class SuperAdmins
    {
        // battletags have to be lowercased!
        private static List<string> ApprovedSuperAdmins =>
            new List<string>
            {
                "floss2xdaily#1987",
                "setcho#2271",
                "modmoto#2809",
                "cepheid#1467",
                "fluxxu#1815",
            };

        public static bool IsSuperAdmin(string battleTag)
        {
            return ApprovedSuperAdmins.Any(p => p == battleTag.ToLower());
        }
    }
}
