using AutoFixture;
using NUnit.Framework;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Handlers
{
    public class RolesCommandHandlerTests
    {
        Fixture _fixture;

        [SetUp]
        public void RolesCommandHandlerTestsSetup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void TestExists()
        {
            Assert.Pass();
        }

        // role creation tests:
        // 
    }
}
