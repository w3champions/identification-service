using MongoDB.Bson.Serialization.Attributes;
using W3ChampionsIdentificationService.DatabaseModels;

namespace W3ChampionsIdentificationService.RolesAndPermissions;

public class Permission : IIdentifiable
{
    [BsonId]
    public string Id => BattleTag;
    public string BattleTag { get; set; }
    public string Description { get; set; }
    public EPermission[] Permissions { get; set; }
    public string Author { get; set; }
}

public enum EPermission {
    Permissions,
    Moderation,
    Queue,
    Logs,
    Maps,
    Tournaments,
    Content,
    Proxies,
}
