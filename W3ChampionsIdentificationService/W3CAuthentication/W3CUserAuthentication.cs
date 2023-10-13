using System;
using System.Collections.Generic;
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

        public static W3CUserAuthentication Create(string battleTag, string privateKey, List<string> permissions, long? bnetId = null)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);

            var isAdmin = permissions.Count > 0;
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
                    new("permissions", permissions != null ? JsonSerializer.Serialize(permissions) : string.Empty,JsonClaimValueTypes.JsonArray),
                    new("bnetId", bnetId?.ToString() ?? "")
                },
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddDays(7)
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new W3CUserAuthentication
            {
                BattleTag = battleTag,
                JWT = token,
                IsAdmin = isAdmin,
                Name = name,
                Permissions = permissions,
                BnetID = bnetId?.ToString(),
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
                var permissions = claims.Claims
                    .Where(claim => claim.Type == "permissions")
                    .Select(x => x.Value)
                    .ToList();

                return new W3CUserAuthentication
                {
                    Name = name,
                    BattleTag = btag,
                    IsAdmin = isAdmin,
                    JWT = jwt,
                    Permissions = permissions,
                    BnetID = claims.Claims
                        .Where(claim => claim.Type == "bnetId")
                        .Select(x => x.Value)
                        .FirstOrDefault()
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
        public List<string> Permissions { get; set; }
        public string BnetID {  get; set; }
    }
}