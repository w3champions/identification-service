using System.Text.Json.Serialization;

namespace W3ChampionsIdentificationService.Blizzard;

public class AuthenticationError
{
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    // Only set for MISSING_WARCRAFT_3 so the launcher can show which account was used;
    // omitted from the JSON entirely when null so other error bodies stay unchanged.
    [JsonPropertyName("battleTag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string BattleTag { get; set; }

    public static AuthenticationError ApiCallFailed()
    {
        return new AuthenticationError
        {
            ErrorCode = "PLAYABLE_TITLES_API_FAILED",
            Message = "Unable to get playable titles"
        };
    }

    public static AuthenticationError MissingWarcraft3(string battleTag)
    {
        return new AuthenticationError
        {
            ErrorCode = "MISSING_WARCRAFT_3",
            Message = "You need to have Warcraft 3 purchased.",
            BattleTag = string.IsNullOrWhiteSpace(battleTag) ? null : battleTag
        };
    }

    public static AuthenticationError MissingPlayableTitlesScope()
    {
        return new AuthenticationError
        {
            ErrorCode = "MISSING_PLAYABLE_TITLES_SCOPE",
            Message = "You need to grant the streaming.titles scope."
        };
    }

    public static AuthenticationError UnsupportedVersion()
    {
        return new AuthenticationError
        {
            ErrorCode = "UNSUPPORTED_VERSION",
            Message = "You need to update your client to the latest version."
        };
    }

    public static AuthenticationError UnknownError()
    {
        return new AuthenticationError
        {
            ErrorCode = "UNKNOWN_ERROR",
            Message = "An unknown error occurred"
        };
    }
}
