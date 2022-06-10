using MongoDB.Bson.Serialization.Attributes;
using W3ChampionsIdentificationService.DatabaseModels;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class Permission : IIdentifiable
    {
        [BsonId]
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
