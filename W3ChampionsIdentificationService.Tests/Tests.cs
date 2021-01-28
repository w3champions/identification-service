using JWT;
using JWT.Serializers;
using NUnit.Framework;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests
{
    public class Tests
    {
        private string _privateKey =
            "MIIEowIBAAKCAQEAsXVkEXqldnZwDotoberO7fwMgR8QBdCmIJYrjBfl+qEXAMFv3UMLtNyWLvZKrObaXB3IH2iy0GLraJdFdAXZgiQdfH9qBgGpuEUOADi26k+sQ4CtHJxvkj6tzZtFf166PGItmZVeyCQkGfHJB9jBvUnFLEAwCJXor29XPm1DS8tRKN8nKxcPa+Mkj5981uWNoluQPxXdhGK/7auxdfc6IfzPPaKc7qWyCG4jNgyNAezqF0pBiCi2klmgHLhgmf1tnsU9vAnz/rYx9w3Y8AgILuEoBW0bwOguqXgOgFjQMGpZGdITanVlJiIiSKB5HXxfi34hLjFiUqlDgZAnhVz9QQIDAQABAoIBAGa/q7MbKlbO+M7TFSIfhLTy7WKN078qMiZIDOrOo5BcWW9MkTKxsUOFPUfvvwkhmWkgrVDSVEaoH9mtTL+C64+YzaHaPE9CAzz4bsTeEbrVas6i4JjiUw+ATy3vK2w2MurnhbOZcwm7M1P6VQXAEV1CK3IVsDooYlPByegRZ5eoKH0ScVES2RuaubT1vKCATue8V6u1jnP08orx/3RB4+iNhmkfCVrjHZWArakEbT0JwDAYsh2HOvSPM1irx2RSfD8XDmdBTYLh4c+aq2KLCl7YyHqSYPws5uSfnSh144ltSH95kaDkYlyjDZgZOSSAOmGD5OF/wA4fwXp2dUBBaekCgYEA0q5uO6hNLQgO59L452fJIWe23XG723pIGPnZaoJfAyfDFVWOypPFUSZQNcvk9yvbUErUps5FsypactSjGk2iXwyzUj8hCJo5c+H67wIyx39tNc0lL8oK57M/uTMT2rmbacBCMgurBiJac8Pa9ksyL7yTgpH7Qp2xHQ/UnpwTu18CgYEA16F7r0S/UXXKBx5Q1uLjDh24QOwNlVSu5RMDY3JdSlK+EjaD4G21luQZPM8G0qpmMVaGnhIibS2ctOYvUL21w/wkXjcxuMeRxfBKAw7cVIwwM/0HLCjbLbhIzzkJWPAk4o2yrzpK0JBTWEBDRQM0pW1f7/MBuHlRlWGOF8Feq18CgYEAmNLEzWuJB/hHb6wetyUWxa+I+1sL7OnnsI5UQqltKEnIfgpA+Pt17yZ+mndbkFv0y2pslM8dyhgX4iISafsUpCP/U9LNMING16N9ta//i60IWDWGGwNRI79novzFEyz96hj3K3xBQ8LZoA3bIDttnSkKS3V/MFRY7H7WOXN38LsCgYAdPXmfN8xVyIo/0t2VdijiBiexLhFdeaFJ0P05pA2AgZ3irF/PtDDWqFG/zDcZpB+9ewVANCdU9MV2RmyOH1uhmvR3jjaA3xcmZ0IyDfbUxDnZIuuxEk0+cun0p8BbvhzSV/KuDlKM27Vhlz1IA3pFIILM8AAAuvrULelt0Tv8MwKBgGNA5W9TB0uEPp2aOqxPQhpnvz+xH2AuVkROz3xYSsWrIqwt9weFJau7MxMWvQyLhGqgfmDTxQrDWcihOet4SOewASbbLWqlP0yI952hb6rSfOOp6YD9PmT8GFKJXmc9Z3IIQm3X52MA2SIqAOrThWDS4H18rXi3aPfpzlfEf+Cl";

        private string _publicKey =
            "MIIBCgKCAQEAsXVkEXqldnZwDotoberO7fwMgR8QBdCmIJYrjBfl+qEXAMFv3UMLtNyWLvZKrObaXB3IH2iy0GLraJdFdAXZgiQdfH9qBgGpuEUOADi26k+sQ4CtHJxvkj6tzZtFf166PGItmZVeyCQkGfHJB9jBvUnFLEAwCJXor29XPm1DS8tRKN8nKxcPa+Mkj5981uWNoluQPxXdhGK/7auxdfc6IfzPPaKc7qWyCG4jNgyNAezqF0pBiCi2klmgHLhgmf1tnsU9vAnz/rYx9w3Y8AgILuEoBW0bwOguqXgOgFjQMGpZGdITanVlJiIiSKB5HXxfi34hLjFiUqlDgZAnhVz9QQIDAQAB";

        private string _wrongPublicKey =
            "MIIBCgKCAQEAwpRjp/3Xh8MtwsYxI6CqnxtHlEclWS5kQ1gbhR6WUUhm+Sizl3E5rgdVo6SxuUEGphF2Vih1NYGEbL4fWNqY+jqhtZb8AA/1qLSOVl9BdDw1nM0upuY0IPl9qPAnrveDjxR8WGdTywW6Hjf30gTkxCbgyISMoEOaKycmWlk+kS3GoonOLGLnGJKT4QniIC5LxOBRyjRYXyhq0Jd8r/bhLeyKVbCSz5mWjQhNe90lV/8oc34GvemlPe1//jufL3ZlJ3rK34axnEgqFEVebpGgYEV+5DUO/3qAWOwtbxiuqqQE7EywaotPFPEhenHxBBKlcRuycKQnBq94w068E2TVbQIDAQAB";

        [Test]
        public void TestPropertyMapping()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, _publicKey);

            Assert.IsTrue(userAuthentication.IsAdmin);
            Assert.AreEqual("modmoto", userAuthentication.Name);
            Assert.AreEqual("modmoto#2809", userAuthentication.BattleTag);
        }

        [Test]
        public void TestJwtTokenGeneration()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, _publicKey);

            Assert.AreEqual(
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJCYXR0bGVUYWciOiJtb2Rtb3RvIzI4MDkiLCJOYW1lIjoibW9kbW90byIsIklzQWRtaW4iOnRydWV9.cdou95TWyIkmTzri5HoCjMWVtLAu2r0DAUfl5rh8e6i6oP89Lq1XsRJ1wxGMr2RrDGKv-vh_6mS-ulmE1zlr0fIk1e1JBwHxCBitmh7rjLfLMZMQXCDoA2z5CJDTg3hxaOWh8I_6F0x9JAaY5tXkwpl07wImoQNthVgoMC3Rt6c70Qa_LnWOos7iAmA2UUXhoZEemMfyCi2EHtCofG8qI8HIDjGHN3WhQmEjCeiBNjcso9JRXpI9ieuaJQi0gDq2FQcaKtdS0R2oXXF-CH9MnuDzeBrNFYqkConWwgWl3fnxO_zTPhqphUyqq4jp_mlW1PdKeGZhXU8FLyByahkyDg",
                userAuthentication.JWT);
        }

        [Test]
        public void JwtCanRetrieveClaims()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, _publicKey);

            var decode = new JwtDecoder(new JsonNetSerializer(), new JwtBase64UrlEncoder()).Decode(userAuthentication.JWT);
            Assert.AreEqual("{\"BattleTag\":\"modmoto#2809\",\"Name\":\"modmoto\",\"IsAdmin\":true}", decode);
        }

        [Test]
        public void JwtCanBeInvalidated()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, _publicKey);

            var decode = W3CUserAuthentication.FromJWT(userAuthentication.JWT, _publicKey);

            Assert.AreEqual("modmoto#2809", decode.BattleTag);
            Assert.AreEqual(true, decode.IsAdmin);
            Assert.AreEqual("modmoto", decode.Name);
            Assert.AreEqual(userAuthentication.JWT, decode.JWT);
        }

        [Test]
        public void InvalidSecretThrows()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, _publicKey);

            Assert.IsNull(W3CUserAuthentication.FromJWT(userAuthentication.JWT, _wrongPublicKey));
        }

        // run this test to generate secrets and copy the values from the tuple.
        [Test]
        public void SecretGeneration()
        {
            var tuple = W3CUserAuthentication.CreatePublicAndPrivateKey();

            Assert.IsNotNull(tuple.Item1); // private key
            Assert.IsNotNull(tuple.Item2); // piblic key
        }
    }
}