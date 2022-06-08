using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
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

        public static W3CUserAuthentication Create(string battleTag, string privateKey, List<string> roles)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);

            var isAdmin = Admins.IsAdmin(battleTag);
            var isSuperAdmin = SuperAdmins.IsSuperAdmin(battleTag);
            var name = battleTag.Split("#")[0];

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
            };

            var jwt = new JwtSecurityToken(
                claims: new Claim[] {
                    new("battleTag", battleTag),
                    new("isAdmin", isAdmin.ToString()),
                    new("name", name),
                    new("isSuperAdmin", isSuperAdmin.ToString()),
                    new("roles", roles != null ? JsonSerializer.Serialize(roles) : string.Empty,JsonClaimValueTypes.JsonArray)
                },
                signingCredentials: signingCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new W3CUserAuthentication
            {
                BattleTag = battleTag,
                JWT = token,
                IsAdmin = isAdmin,
                IsSuperAdmin = isSuperAdmin,
                Name = name,
                Roles = roles,
            };
        }

        public static W3CUserAuthentication FromJWT(string jwt, string publicKey)
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey);

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
                

                var btag = claims.Claims.First(c => c.Type == "battleTag").Value;
                var isAdmin = Boolean.Parse(claims.Claims.First(c => c.Type == "isAdmin").Value);
                var name = claims.Claims.First(c => c.Type == "name").Value;
                var isSuperAdmin = Boolean.Parse(claims.Claims.First(c => c.Type == "isSuperAdmin").Value);
                var roles = claims.Claims
                    .Where(claim => claim.Type == "roles")
                    .Select(x => x.Value)
                    .ToList();

                return new W3CUserAuthentication
                {
                    Name = name,
                    BattleTag = btag,
                    IsAdmin = isAdmin,
                    JWT = jwt,
                    IsSuperAdmin = isSuperAdmin,
                    Roles = roles,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string JWT { get; set; }
        public string BattleTag { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<string> Roles { get; set; }
    }
}