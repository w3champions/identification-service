using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class OidcClaimMapperTests
{
    [TestCase("Modmoto#2809", "modmoto-2809@w3champions.invalid")]
    [TestCase("Player#12345", "player-12345@w3champions.invalid")]
    [TestCase("UPPER#999",   "upper-999@w3champions.invalid")]
    public void BattleTagToSyntheticEmail_HashReplacedWithDash_LowercaseLocalPart(
        string battleTag, string expectedEmail)
    {
        var result = OidcClaimMapper.BattleTagToSyntheticEmail(battleTag);
        Assert.AreEqual(expectedEmail, result);
    }

    [Test]
    public void BattleTagToSyntheticEmail_NullInput_ThrowsArgumentException()
    {
        Assert.Throws<System.ArgumentException>(() =>
            OidcClaimMapper.BattleTagToSyntheticEmail(null));
    }

    [Test]
    public void SyntheticEmail_DoesNotContainHash()
    {
        var email = OidcClaimMapper.BattleTagToSyntheticEmail("Test#1");
        Assert.IsFalse(email.Contains('#'));
    }

    [Test]
    public void SyntheticEmail_DomainIsCorrect()
    {
        var email = OidcClaimMapper.BattleTagToSyntheticEmail("Test#1");
        StringAssert.EndsWith("@w3champions.invalid", email);
    }
}
