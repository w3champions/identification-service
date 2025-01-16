using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using W3ChampionsIdentificationService.DatabaseModels;

namespace W3ChampionsIdentificationService.RolesAndPermissions;

public class User : IIdentifiable
{
    [BsonId]
    public string Id { get; set; }
    public List<string> Roles { get; set; }

    public string BnetId { get; set; }
}
