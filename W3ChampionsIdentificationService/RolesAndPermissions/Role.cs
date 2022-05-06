using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class Role : IIdentifiable
    {
        [BsonId]
        [JsonIgnore]
        public string Id => Name;
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
