using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class User : IIdentifiable
    {
        [BsonId]
        public string Id { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class UserDTO
    {
        public string BattleTag { get; set; }
        public List<string> Roles { get; set; }
    }
}
