using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class OidcClaimMapperTests
{
    [TestCase("Modmoto#2809", "modmoto-2809@w3champions.invalid")]
    [TestCase("Player#12345", "player-12345@w3champions.invalid")]
    [TestCase("UPPER#999", "upper-999@w3champions.invalid")]
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

    [TestCase("Ümläut#12", TestName = "Accented_Latin_falls_back_to_ascii_hash")]
    [TestCase("玩家#7", TestName = "CJK_falls_back_to_ascii_hash")]
    [TestCase("Кирилл#3", TestName = "Cyrillic_falls_back_to_ascii_hash")]
    public void NonAsciiBattleTag_ProducesAsciiHashEmail(string battleTag)
    {
        var email = OidcClaimMapper.BattleTagToSyntheticEmail(battleTag);

        StringAssert.IsMatch(@"^[a-z0-9._-]+@w3champions\.invalid$", email);
        StringAssert.StartsWith("u-", email);
        Assert.IsFalse(email.Contains('#'));
    }

    [Test]
    public void NonAsciiBattleTags_AreInjective_DistinctInputsProduceDistinctEmails()
    {
        var first = OidcClaimMapper.BattleTagToSyntheticEmail("Ümläut#12");
        var second = OidcClaimMapper.BattleTagToSyntheticEmail("玩家#7");

        Assert.AreNotEqual(first, second);
    }

    [Test]
    public void NonAsciiBattleTag_IsDeterministic_SameInputProducesSameEmail()
    {
        var first = OidcClaimMapper.BattleTagToSyntheticEmail("Кирилл#3");
        var second = OidcClaimMapper.BattleTagToSyntheticEmail("Кирилл#3");

        Assert.AreEqual(first, second);
    }
}
