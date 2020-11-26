using System;
using W3ChampionsIdentificationService.Authorization;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class W3CUserAuthentication : IIdentifiable
    {
        public static W3CUserAuthentication Create(string battletag)
        {
            return new W3CUserAuthentication
            {
                Token = Guid.NewGuid().ToString(),
                BattleTag = battletag
            };
        }

        public string Token { get; set; }
        public string BattleTag { get; set; }
        public string Name => BattleTag.Split("#")[0];
        public Boolean isAdmin => Admins.IsAdmin(BattleTag);
        public string Id => BattleTag;
    }
}