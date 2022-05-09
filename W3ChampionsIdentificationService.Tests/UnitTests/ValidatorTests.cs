using NUnit.Framework;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using Moq;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Validator
{
    public class ValidatorTests
    {
        public RolesAndPermissionsValidator _validator;
        [SetUp]
        public void Setup()
        {
            _validator = new RolesAndPermissionsValidator(null);
        }

        [Test]
        [TestCase("1", "canMute", "Can mute a player", TestName = "CorrectlyFormattedPermission_Success")]
        [TestCase("", "canMute", "Can mute a player", TestName = "IdIsEmptyString_Throws400")]
        [TestCase(null, "canMute", "Can mute a player", TestName = "IdIsNull_Throws400")]
        [TestCase("1", "", "Can mute a player", TestName = "NameIsEmptyString_Throws400")]
        [TestCase("1", null, "Can mute a player", TestName = "NameIsNull_Throws400")]
        [TestCase("1", "canMute", "", TestName = "DescriptionIsEmptyString_Throws400")]
        [TestCase("1", "canMute", null, TestName = "DescriptionIsNull_Throws400")]
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
    }
}
