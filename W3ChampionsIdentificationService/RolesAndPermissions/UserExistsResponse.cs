using System.Text.Json.Serialization;

namespace W3ChampionsIdentificationService.RolesAndPermissions;

public class UserExistsResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}
