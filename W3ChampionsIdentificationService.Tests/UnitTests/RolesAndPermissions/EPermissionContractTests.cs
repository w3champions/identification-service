using System;
using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;

namespace W3ChampionsIdentificationService.Tests.UnitTests.RolesAndPermissions;

// The numeric values and names must stay identical in website-backend, chat-service and the website.
[TestFixture]
public class EPermissionContractTests
{
    [TestCase("Warnings", 11)]
    [TestCase("Jobs", 12)]
    [TestCase("CommercialLicense", 13)]
    public void PermissionHasContractValue(string name, int expectedValue)
    {
        Assert.IsTrue(Enum.TryParse<EPermission>(name, out var permission), $"{name} is missing from EPermission");
        Assert.AreEqual(expectedValue, (int)permission);
    }
}
