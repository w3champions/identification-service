using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using W3ChampionsIdentificationService.DatabaseModels;

namespace W3ChampionsIdentificationService.RolesAndPermissions;

public class User : IIdentifiable
{
    private string _id;

    [BsonId]
    public string Id
    {
        get => _id;
        set
        {
            _id = value;
            IdNormalized = value?.ToLowerInvariant();
        }
    }

    [BsonIgnoreIfNull]
    public string IdNormalized { get; set; }

    public List<string> Roles { get; set; }

    public string BnetId { get; set; }
}
