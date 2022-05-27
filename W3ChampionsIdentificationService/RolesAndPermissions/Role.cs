using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class Role : IIdentifiable
    {
        [BsonId]
        public string Id { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; }
    }
}
