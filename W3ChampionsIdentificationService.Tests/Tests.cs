using JWT;
using JWT.Exceptions;
using JWT.Serializers;
using NUnit.Framework;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests
{
    public class Tests
    {
        [Test]
        public void TestPropertyMapping()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809");

            Assert.IsTrue(userAuthentication.IsAdmin);
            Assert.AreEqual("modmoto", userAuthentication.Name);
            Assert.AreEqual("modmoto#2809", userAuthentication.BattleTag);
        }

        [Test]
        public void TestJwtTokenGeneration()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809");

            Assert.AreEqual(
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJCYXR0bGVUYWciOiJtb2Rtb3RvIzI4MDkiLCJOYW1lIjoibW9kbW90byIsIklzQWRtaW4iOnRydWV9.lMT680d_JC2zqHvhFxTJs3eLEu2SM0jVQ7dup_mW4fA",
                userAuthentication.JWT);
        }

        [Test]
        public void JwtCanRetrieveClaims()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809");

            var decode = new JwtDecoder(new JsonNetSerializer(), new JwtBase64UrlEncoder()).Decode(userAuthentication.JWT);
            Assert.AreEqual("{\"BattleTag\":\"modmoto#2809\",\"Name\":\"modmoto\",\"IsAdmin\":true}", decode);
        }

        [Test]
        public void JwtCanBeInvalidated()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809");

            var decode = W3CUserAuthentication.FromJWT(userAuthentication.JWT);

            Assert.AreEqual("modmoto#2809", decode.BattleTag);
            Assert.AreEqual(true, decode.IsAdmin);
            Assert.AreEqual("modmoto", decode.Name);
            Assert.AreEqual(userAuthentication.JWT, decode.JWT);
        }

        [Test]
        public void InvalidSecretThrows()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809");

            Assert.IsNull(W3CUserAuthentication.FromJWT(userAuthentication.JWT, "NotTheSecret"));
        }
    }
}