using NUnit.Framework;
using System.Linq;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests.Unit
{
    [TestFixture]
    public class JWTTests
    {
        private readonly string _privateKey = "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n";
        private readonly string _publicKey = "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA3l8idcfxcViOdQ4nZZwX\nsP6l42CTQmTyPT3jSOhFvm+YvK0bvoHw16ITYYg9CYni/aDzi8oulrI65cr8xFKg\ncojZnnbTDJk9YLLUznwxBtqub7MNF89t1+pfUgUp2H3zGzoDSl31A205mwBDRXOd\n5Zn97Fi8268Doy2/kIYKgycd7BaC+MqdecJ0KRZMpWbsA5QOewK4zmQyf6hlGLPp\nBNYkE7n2vg3RxeVsw1duEfD39Zh6kBAuTLTJDTxuDXlSuf8vH8NvPQ3ROTGl6cSH\nz8YOUz2em5L8wIYAW8W0tAamOTw9T2wEHEFpy2qpOuYWXtk3v0x3sfplBuzm8LE/\nDusuSyipoS0ZJJQGsOA/G2oaOFwRew5q9M+NxlpCFJEuSNHKi609W+FjX04sxovx\nuEjyp9RNeQ8BSeiad5kSXDLSs9Dintn8oulzil5pA+ccJ8PldJYRqrTjx+lB+STO\nnsgtg1esTDVXn6HOva857LVv2AN5sT8siXTBcXRXLjDxWWoI5N3xrf4Vbh+p6P0c\nWoXB7puyt3IKb68Rk0DcTC5WW1vh4neVslL8uhxwprS7J3NvBqY4ds/zfHj+3q2P\nvDEX2DOgXidjIQjlIdjF0S+FzZ8wJd3LEcPXjDhqefdOSQDdG2G7dqBrDKTDpEQt\n2+Rj5s0owdQbrCEuvFe92jECAwEAAQ==\n-----END PUBLIC KEY-----\n";

        private readonly string _wrongPublicKey =
            "MIIBCgKCAQEAwpRjp/3Xh8MtwsYxI6CqnxtHlEclWS5kQ1gbhR6WUUhm+Sizl3E5rgdVo6SxuUEGphF2Vih1NYGEbL4fWNqY+jqhtZb8AA/1qLSOVl9BdDw1nM0upuY0IPl9qPAnrveDjxR8WGdTywW6Hjf30gTkxCbgyISMoEOaKycmWlk+kS3GoonOLGLnGJKT4QniIC5LxOBRyjRYXyhq0Jd8r/bhLeyKVbCSz5mWjQhNe90lV/8oc34GvemlPe1//jufL3ZlJ3rK34axnEgqFEVebpGgYEV+5DUO/3qAWOwtbxiuqqQE7EywaotPFPEhenHxBBKlcRuycKQnBq94w068E2TVbQIDAQAB";

        [Test]
        public void TestPropertyMapping()
        {
            string[] permissions = { "ban", "mute", "admin" };
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, permissions.ToList());

            Assert.IsTrue(userAuthentication.IsAdmin);
            Assert.AreEqual("modmoto", userAuthentication.Name);
            Assert.AreEqual("modmoto#2809", userAuthentication.BattleTag);
            Assert.AreEqual(3, userAuthentication.Permissions.Count);
            Assert.IsTrue(userAuthentication.Permissions.Contains("ban"));
            Assert.IsTrue(userAuthentication.Permissions.Contains("mute"));
            Assert.IsTrue(userAuthentication.Permissions.Contains("admin"));
            Assert.IsTrue(userAuthentication.IsSuperAdmin);
        }

        [Test]
        public void TestJwtTokenGeneration()
        {
            string[] roles = { "ban", "mute", "admin" };
            var userAuthentication = W3CUserAuthentication.Create("notsuperadmin#2809", _privateKey, roles.ToList());

            Assert.Greater(userAuthentication.JWT.Length, 800);
            Assert.IsFalse(userAuthentication.IsSuperAdmin);
            Assert.AreEqual(3, userAuthentication.Permissions.Count);
        }

        [Test]
        public void JwtCanBeValidated()
        {
            string[] permissions = { "ban", "mute", "admin" };
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, permissions.ToList());

            var decode = W3CUserAuthentication.FromJWT(userAuthentication.JWT, _publicKey);

            Assert.AreEqual("modmoto#2809", decode.BattleTag);
            Assert.AreEqual(true, decode.IsAdmin);
            Assert.AreEqual("modmoto", decode.Name);
            Assert.AreEqual(userAuthentication.JWT, decode.JWT);
        }

        [Test]
        public void InvalidSecretThrows()
        {
            string[] permissions = { "ban", "mute", "admin" };
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, permissions.ToList());

            Assert.IsNull(W3CUserAuthentication.FromJWT(userAuthentication.JWT, _wrongPublicKey));
        }

        // run this test to generate secrets and copy the values from the tuple.
        /*[Test]
        public void SecretGeneration()
        {
            var tuple = W3CUserAuthentication.CreatePublicAndPrivateKey();

            Assert.IsNotNull(tuple.Item1); // private key
            Assert.IsNotNull(tuple.Item2); // public key
        }*/
    }
}