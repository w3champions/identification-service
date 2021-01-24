using System.Text.Json.Serialization;
using JWT;
using JWT.Builder;
using JWT.Serializers;
using NUnit.Framework;
using W3ChampionsIdentificationService.Authorization;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests
{
    public class Tests
    {
        [Test]
        public void TestError()
        {
            var errorResult = new ErrorResult("jeah");
            Assert.AreEqual("jeah", errorResult.Error);
        }

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

            Assert.AreEqual("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJiYXR0bGUtdGFnIjoibW9kbW90byMyODA5IiwibmFtZSI6Im1vZG1vdG8iLCJpcy1hZG1pbiI6dHJ1ZX0.r-MxdQGuLU67ySQm7GapVT0NcsiDmitWXmmTUTynrBA", userAuthentication.JwtToken);
        }

        [Test]
        public void JwtCanRetrieveClaims()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809");

            var decode = new JwtDecoder(new JsonNetSerializer(), new JwtBase64UrlEncoder()).Decode(userAuthentication.JwtToken);
            Assert.AreEqual("{\"battle-tag\":\"modmoto#2809\",\"name\":\"modmoto\",\"is-admin\":true}", decode);
        }
    }
}