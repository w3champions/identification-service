using System;
using System.Text.Json;
using JWT.Algorithms;
using JWT.Builder;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class W3CUserAuthentication
    {
        public static W3CUserAuthentication Create(string battleTag, string jwtSecret = "secret")
        {
            var isAdmin = Admins.IsAdmin(battleTag);
            var name = battleTag.Split("#")[0];
            var jwt = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(jwtSecret)
                .AddClaim("BattleTag", battleTag)
                .AddClaim("Name", name)
                .AddClaim("IsAdmin", isAdmin)
                .Encode();
            return new W3CUserAuthentication
            {
                BattleTag = battleTag,
                JWT = jwt,
                IsAdmin = isAdmin,
                Name = name
            };
        }

        public string JWT { get; set; }

        public static W3CUserAuthentication FromJWT(string jwt, string jwtSecret = "secret")
        {
            try
            {
                var decode = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(jwtSecret)
                    .MustVerifySignature()
                    .Decode(jwt);

                var user = JsonSerializer.Deserialize<W3CUserAuthentication>(decode);
                user.JWT = jwt;
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string BattleTag { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }
}