using System;
using JWT.Algorithms;
using JWT.Builder;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class W3CUserAuthentication
    {
        private string _jwtTokenSecret;

        public static W3CUserAuthentication Create(string battleTag, string jwtSecret = "secret")
        {
            return new()
            {
                BattleTag = battleTag,
                _jwtTokenSecret = jwtSecret
            };
        }

        public string BattleTag { get; set; }
        public string Name => BattleTag.Split("#")[0];

        public Boolean IsAdmin => Admins.IsAdmin(BattleTag);

        public string JwtToken =>
            new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_jwtTokenSecret)
                .AddClaim("battle-tag", BattleTag)
                .AddClaim("name", Name)
                .AddClaim("is-admin", IsAdmin)
                .Encode();
    }
}