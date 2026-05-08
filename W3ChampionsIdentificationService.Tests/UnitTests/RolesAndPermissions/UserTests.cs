using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;

namespace W3ChampionsIdentificationService.Tests.UnitTests.RolesAndPermissions;

[TestFixture]
public class UserTests
{
    [Test]
    public void SetId_PopulatesIdNormalized()
    {
        var user = new User { Id = "TORREN#11438" };

        Assert.AreEqual("TORREN#11438", user.Id);
        Assert.AreEqual("torren#11438", user.IdNormalized,
            "Setting Id must auto-populate IdNormalized to the lowercased form.");
    }

    [Test]
    public void SetIdToNull_IdNormalizedIsNull()
    {
        var user = new User { Id = "Foo#1234" };
        Assert.AreEqual("foo#1234", user.IdNormalized);

        user.Id = null;

        Assert.IsNull(user.Id);
        Assert.IsNull(user.IdNormalized);
    }

    [Test]
    public void SetId_AlreadyLowercase_IdNormalizedMatchesId()
    {
        var user = new User { Id = "lowtag#0001" };

        Assert.AreEqual("lowtag#0001", user.Id);
        Assert.AreEqual("lowtag#0001", user.IdNormalized);
    }
}
