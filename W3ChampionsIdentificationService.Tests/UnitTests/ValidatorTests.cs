using NUnit.Framework;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.Tests.UnitTests
{
    [TestFixture]
    public class ValidatorTests
    {
        public RolesAndPermissionsValidator _validator;
        [SetUp]
        public void Setup()
        {
            _validator = new RolesAndPermissionsValidator(null);
        }

        [Test]
        [TestCase("1", "canMute", "Can mute a player", TestName = "PermissionValidationHttp_CorrectlyFormattedPermission_Success")]
        [TestCase("", "canMute", "Can mute a player", TestName = "PermissionValidationHttp_IdIsEmptyString_Throws400")]
        [TestCase(null, "canMute", "Can mute a player", TestName = "PermissionValidationHttp_IdIsNull_Throws400")]
        [TestCase("1", "", "Can mute a player", TestName = "PermissionValidationHttp_NameIsEmptyString_Throws400")]
        [TestCase("1", null, "Can mute a player", TestName = "PermissionValidationHttp_NameIsNull_Throws400")]
        [TestCase("1", "canMute", "", TestName = "PermissionValidationHttp_DescriptionIsEmptyString_Throws400")]
        [TestCase("1", "canMute", null, TestName = "PermissionValidationHttp_DescriptionIsNull_Throws400")]
        public void PermissionsValidationHttp_ThrowsCorrectErrors(string id, string name, string description)
        {
            var permission = new Permission()
            {
                Id = id,
                Name = name,
                Description = description
            };

            if (!string.IsNullOrEmpty(permission.Id) && 
                !string.IsNullOrEmpty(permission.Name) && 
                !string.IsNullOrEmpty(permission.Description))
            {
                Assert.DoesNotThrow(() => _validator.ValidatePermissionHttp(permission));
                Assert.Pass();
            }

            var exception = Assert.Throws<HttpException>(() => _validator.ValidatePermissionHttp(permission));
            Assert.AreEqual(400, exception.StatusCode);
            StringAssert.Contains("cannot be null or empty", exception.Message);
        }

        [Test]
        [TestCase("1", "moderator", "access to moderation tools", TestName = "RoleValidationHttp_CorrectlyFormattedRole_Success")]
        [TestCase("", "moderator", "access to moderation tools", TestName = "RoleValidationHttp_IdIsEmptyString_Throws400")]
        [TestCase(null, "moderator", "access to moderation tools", TestName = "RoleValidationHttp_IdIsNull_Throws400")]
        [TestCase("1", "", "access to moderation tools", TestName = "RoleValidationHttp_NameIsEmptyString_Throws400")]
        [TestCase("1", null, "access to moderation tools", TestName = "RoleValidationHttp_NameIsNull_Throws400")]
        [TestCase("1", "moderator", "", TestName = "RoleValidationHttp_DescriptionIsEmptyString_Throws400")]
        [TestCase("1", "moderator", null, TestName = "RoleValidationHttp_DescriptionIsNull_Throws400")]
        public void RoleValidationHttp_ThrowsCorrectErrors(string id, string name, string description)
        {
            var role = new Role()
            {
                Id = id,
                Name = name,
                Description = description
            };

            if (!string.IsNullOrEmpty(role.Id) &&
                !string.IsNullOrEmpty(role.Name) &&
                !string.IsNullOrEmpty(role.Description))
            {
                Assert.DoesNotThrow(() => _validator.ValidateRoleHttp(role));
                Assert.Pass();
            }

            var exception = Assert.Throws<HttpException>(() => _validator.ValidateRoleHttp(role));
            Assert.AreEqual(400, exception.StatusCode);
            StringAssert.Contains("cannot be null or empty", exception.Message);
        }

        [Test]
        [TestCase("1", "Cepheid#1467", TestName = "UserValidationHttp_CorrectlyFormattedPermission_Success")]
        [TestCase("", "Cepheid#1467", TestName = "UserValidationHttp_IdIsEmptyString_Throws400")]
        [TestCase(null, "Cepheid#1467", TestName = "UserValidationHttp_IdIsNull_Throws400")]
        [TestCase("1", "", TestName = "UserValidationHttp_BattleTagIsEmptyString_Throws400")]
        [TestCase("1", null, TestName = "UserValidationHttp_BattleTagIsNull_Throws400")]
        [TestCase("1", "incorrectlyFormattedBattletag#1", TestName = "UserValidationHttp_BattleTagIsIncorrectFormat_Throws400")]
        public void UserValidationHttp_ThrowsCorrectErrors(string id, string tag)
        {
            var permission = new User()
            {
                Id = id,
                BattleTag = tag,
                Permissions = new List<string>()
            };

            if (!string.IsNullOrEmpty(permission.Id) && 
                permission.BattleTag == "Cepheid#1467")
            {
                Assert.DoesNotThrow(() => _validator.ValidateUserHttp(permission));
                Assert.Pass();
            }

            if (permission.BattleTag == "incorrectlyFormattedBattletag#1")
            {
                var ex = Assert.Throws<HttpException>(() => _validator.ValidateUserHttp(permission));
                Assert.AreEqual("BattleTag is not valid", ex.Message);
                Assert.AreEqual(400, ex.StatusCode);
                Assert.Pass();
            }

            var exception = Assert.Throws<HttpException>(() => _validator.ValidateUserHttp(permission));
            Assert.AreEqual(400, exception.StatusCode);
            StringAssert.Contains("cannot be null or empty", exception.Message);
        }
    }
}
