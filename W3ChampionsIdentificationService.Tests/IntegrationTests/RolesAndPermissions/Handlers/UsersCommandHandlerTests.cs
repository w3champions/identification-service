using AutoFixture;
using NUnit.Framework;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Handlers
{
    public class UsersCommandHandlerTests
    {
        Fixture _fixture;

        [SetUp]
        public void UsersCommandHandlerTestsSetup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void TestExists()
        {
            Assert.Pass();
        }
    }
}
