using System.Linq;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using W3ChampionsIdentificationService.DataProtection;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests.DataProtection;

// Exercises the real Mongo-backed DataProtection key-ring repository. Uses the shared
// IntegrationTestBase harness (live MongoDB, DB dropped in [SetUp]) so the store/read-back
// round-trip is genuinely verified rather than mocked.
public class MongoXmlRepositoryTests : IntegrationTestBase
{
    private const string CollectionName = "DataProtectionKeys";

    private MongoXmlRepository CreateRepository() =>
        new(CreateClient().GetCollection<BsonDocument>(CollectionName));

    [Test]
    public void StoreElement_ThenGetAllElements_RoundTripsTheXml()
    {
        var repo = CreateRepository();
        var element = new XElement("key", new XAttribute("id", "abc"), new XElement("descriptor", "value"));

        repo.StoreElement(element, friendlyName: "key-abc");

        var all = repo.GetAllElements();
        Assert.AreEqual(1, all.Count, "Exactly one stored element must be read back.");
        Assert.AreEqual(element.ToString(SaveOptions.DisableFormatting), all.Single().ToString(SaveOptions.DisableFormatting));
    }

    [Test]
    public void StoreElement_SameFriendlyNameTwice_Upserts_NoDuplicate()
    {
        var repo = CreateRepository();
        var original = new XElement("key", new XAttribute("id", "abc"), new XElement("payload", "v1"));
        var updated = new XElement("key", new XAttribute("id", "abc"), new XElement("payload", "v2"));

        repo.StoreElement(original, friendlyName: "key-abc");
        repo.StoreElement(updated, friendlyName: "key-abc");

        var all = repo.GetAllElements();
        Assert.AreEqual(1, all.Count, "Re-storing the same friendlyName must upsert in place, not duplicate.");
        StringAssert.Contains("v2", all.Single().ToString(), "The latest value must win.");
    }

    [Test]
    public void StoreElement_DistinctKeys_AllReadBack()
    {
        var repo = CreateRepository();
        repo.StoreElement(new XElement("key", new XAttribute("id", "1")), friendlyName: "key-1");
        repo.StoreElement(new XElement("key", new XAttribute("id", "2")), friendlyName: "key-2");

        var all = repo.GetAllElements();
        Assert.AreEqual(2, all.Count, "Distinct keys must each be persisted and read back.");
    }

    [Test]
    public void StoreElement_NullFriendlyName_StillPersists()
    {
        var repo = CreateRepository();
        repo.StoreElement(new XElement("key", new XAttribute("id", "x")), friendlyName: null);

        var all = repo.GetAllElements();
        Assert.AreEqual(1, all.Count, "An element with no friendlyName must still be persisted exactly once.");
    }
}
