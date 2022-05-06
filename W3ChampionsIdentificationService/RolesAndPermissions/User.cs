using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class User : IIdentifiable
    {
        [BsonId]
        public string Id { get; set; }
        public string BattleTag { get; set; }
        public List<Role> Roles { get; set; }
        public bool SuperAdmin { get; set; }
    }
}
