using NUnit.Framework;
using W3ChampionsIdentificationService.Authorization;

namespace W3ChampionsIdentificationService.Tests
{
    public class Tests
    {
        [Test]
        public void TestError()
        {
            var errorResult = new ErrorResult("jeah");
            Assert.AreEqual("jeah", errorResult.Error);
        }
    }
}