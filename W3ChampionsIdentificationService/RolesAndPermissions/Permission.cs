using MongoDB.Bson.Serialization.Attributes;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class Permission : IIdentifiable
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
