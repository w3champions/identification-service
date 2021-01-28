using System;
using System.Security.Cryptography;
using System.Text.Json;
using JWT.Algorithms;
using JWT.Builder;

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

        public static W3CUserAuthentication Create(string battleTag, string privateKey, string publicKey)
        {
            var publicKeyAsBytes = Convert.FromBase64String(publicKey);
            var privateKeyAsBytes = Convert.FromBase64String(privateKey);

            var rsaPrivate = RSA.Create();
            rsaPrivate.ImportRSAPrivateKey(privateKeyAsBytes, out _);

            var rsaPublic = RSA.Create();
            rsaPublic.ImportRSAPublicKey(publicKeyAsBytes, out _);


            var isAdmin = Admins.IsAdmin(battleTag);
            var name = battleTag.Split("#")[0];

            var jwt = new JwtBuilder()
                .WithAlgorithm(new RS256Algorithm(rsaPublic, rsaPrivate))
                .MustVerifySignature()
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

        public static W3CUserAuthentication FromJWT(string jwt, string publicKey)
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

                var decode = new JwtBuilder()
                    .WithAlgorithm(new RS256Algorithm(rsa))
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