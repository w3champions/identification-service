using System;
using System.Text.Json.Serialization;
using JWT.Algorithms;
using JWT.Builder;
using W3ChampionsIdentificationService.Authorization;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class W3CUserAuthentication : IIdentifiable
    {
        private static readonly string JwtTokenSecret = Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET") ?? "secret";
        public static W3CUserAuthentication Create(string battleTag)
        {
            return new()
            {
                BattleTag = battleTag
            };
        }

        public string BattleTag { get; set; }
        public string Name => BattleTag.Split("#")[0];

        public Boolean IsAdmin => Admins.IsAdmin(BattleTag);

        [JsonIgnore]
        public string Id => BattleTag;

        public string JwtToken =>
            new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(JwtTokenSecret)
                .AddClaim("battle-tag", BattleTag)
                .AddClaim("name", Name)
                .AddClaim("is-admin", IsAdmin)
                .Encode();
    }
}