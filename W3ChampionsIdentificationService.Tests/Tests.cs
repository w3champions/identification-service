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
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey);

            Assert.IsTrue(userAuthentication.IsAdmin);
            Assert.AreEqual("modmoto", userAuthentication.Name);
            Assert.AreEqual("modmoto#2809", userAuthentication.BattleTag);
        }

        [Test]
        public void TestJwtTokenGeneration()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey);

            Assert.AreEqual(
                "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJCYXR0bGVUYWciOiJtb2Rtb3RvIzI4MDkiLCJJc0FkbWluIjoiVHJ1ZSIsIk5hbWUiOiJtb2Rtb3RvIn0.p7MtNKivSkLfad6a-KEE_brGyrJg81Zo21KqmdytC_n-J_tKffMb_DPotxRd10rEseo_OuAlsZc0Iv3jUU5IPi9nyOgdxjGvzqI6YgZMgJlE9l2XDHpuCkcUdr_us5pv9R_qJDLT8jv9XJynboynrPJzsGFTg2us9mtO9PZ4xid2dOoV7sfJauELhOoXlrQe_Pbgp8J3i5h2tQSJ7ZByubY5wfkJvp6-03leIYGJhMMmCSsc4bRTsGimZKMZhOTvwJaV4OUcemZz3onsZ7uSCL5Fwl46L0mWCkBIJsPUuS0MNS3LLqYxuUlt4unb1G7VT7XUtfR8RgIKQvt1nwQvUA",
                userAuthentication.JWT);
        }

        [Test]
        public void JwtCanBeValidated()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey);

            var decode = W3CUserAuthentication.FromJWT(userAuthentication.JWT, _publicKey);

            Assert.AreEqual("modmoto#2809", decode.BattleTag);
            Assert.AreEqual(true, decode.IsAdmin);
            Assert.AreEqual("modmoto", decode.Name);
            Assert.AreEqual(userAuthentication.JWT, decode.JWT);
        }

        [Test]
        public void InvalidSecretThrows()
        {
            var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey);

            Assert.IsNull(W3CUserAuthentication.FromJWT(userAuthentication.JWT, _wrongPublicKey));
        }

        // run this test to generate secrets and copy the values from the tuple.
        [Test]
        public void SecretGeneration()
        {
            var tuple = W3CUserAuthentication.CreatePublicAndPrivateKey();

            Assert.IsNotNull(tuple.Item1); // private key
            Assert.IsNotNull(tuple.Item2); // public key
        }
    }
}