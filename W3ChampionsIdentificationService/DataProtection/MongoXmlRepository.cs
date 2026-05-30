using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace W3ChampionsIdentificationService.DataProtection;

/// <summary>
/// An <see cref="IXmlRepository"/> that persists the ASP.NET Core DataProtection key ring
/// to a MongoDB collection so it survives restarts and is shared across replicas.
///
/// Why this exists: the IdP session cookie (<c>__Host-w3c-idp-session</c>) is encrypted with
/// DataProtection. The default key ring is EPHEMERAL and per-instance, so a cookie set by one
/// replica (or before a restart) cannot be decrypted by another — the SSO handoff would set the
/// cookie on one instance and the redirected /connect/authorize could land on another that fails
/// to authenticate it, looping the user back through login. Persisting the key ring to the
/// already-present MongoDB makes it durable and shared with no new infrastructure.
///
/// This is distinct from OIDC_ENCRYPTION_KEY_PEM, which protects OpenIddict authorization codes;
/// this protects the ASP.NET cookie key ring.
/// </summary>
public sealed class MongoXmlRepository(IMongoCollection<BsonDocument> collection) : IXmlRepository
{
    // DataProtection stores each key as a top-level <key> XML element. We persist one Mongo
    // document per element: { _id: <friendlyName or generated>, xml: "<key>...</key>" }.
    private const string XmlField = "xml";

    private readonly IMongoCollection<BsonDocument> _collection = collection;

    public IReadOnlyCollection<XElement> GetAllElements() =>
        _collection
            .Find(FilterDefinition<BsonDocument>.Empty)
            .ToList()
            .Where(doc => doc.Contains(XmlField) && doc[XmlField].IsString)
            .Select(doc => XElement.Parse(doc[XmlField].AsString))
            .ToList();

    public void StoreElement(XElement element, string friendlyName)
    {
        // DataProtection supplies the key id as friendlyName; use it as the document _id so a
        // re-store of the same key upserts in place rather than duplicating. When absent, fall
        // back to a generated ObjectId so the element is still persisted exactly once.
        var id = string.IsNullOrEmpty(friendlyName)
            ? ObjectId.GenerateNewId().ToString()
            : friendlyName;

        var document = new BsonDocument
        {
            ["_id"] = id,
            [XmlField] = element.ToString(SaveOptions.DisableFormatting),
        };

        _collection.ReplaceOne(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            document,
            new ReplaceOptions { IsUpsert = true });
    }
}
