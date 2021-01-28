using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class W3CUserAuthentication
    {
        public static Tuple<string, string> CreatePublicAndPrivateKey()
        {
            var rsa = RSA.Create();
            var priv = rsa.ExportRSAPrivateKey();
            var pub = rsa.ExportRSAPublicKey();

            string privText = Convert.ToBase64String(priv);
            string pubText = Convert.ToBase64String(pub);

            return new Tuple<string, string>(privText, pubText);
        }

        public static W3CUserAuthentication Create(string battleTag, string privateKey)
        {
            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            var isAdmin = Admins.IsAdmin(battleTag);
            var name = battleTag.Split("#")[0];

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
            };

            var jwt = new JwtSecurityToken(
                claims: new Claim[] {
                    new Claim("BattleTag", battleTag),
                    new Claim("IsAdmin", isAdmin.ToString()),
                    new Claim("Name", name)
                },
                signingCredentials: signingCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new W3CUserAuthentication
            {
                BattleTag = battleTag,
                JWT = token,
                IsAdmin = isAdmin,
                Name = name
            };
        }

        public string JWT { get; set; }

        public static W3CUserAuthentication FromJWT(string jwt, string publicKey)
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateTokenReplay = false,
                    ValidateActor = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa)
                };

                var handler = new JwtSecurityTokenHandler();
                var claims = handler.ValidateToken(jwt, validationParameters, out _);
                var btag = claims.Claims.First(c => c.Type == "BattleTag").Value;
                var isAdmin = Boolean.Parse(claims.Claims.First(c => c.Type == "IsAdmin").Value);
                var name = claims.Claims.First(c => c.Type == "Name").Value;

                return new W3CUserAuthentication
                {
                    Name = name,
                    BattleTag = btag,
                    IsAdmin = isAdmin,
                    JWT = jwt
                };
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