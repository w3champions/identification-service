using System.Text.Json.Serialization;

namespace W3ChampionsIdentificationService.Blizzard;

public class AuthenticationError
{
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    public static AuthenticationError ApiCallFailed()
    {
        return new AuthenticationError
        {
            ErrorCode = "PLAYABLE_TITLES_API_FAILED",
            Message = "Unable to get playable titles"
        };
    }

    public static AuthenticationError MissingWarcraft3()
    {
        return new AuthenticationError
        {
            ErrorCode = "MISSING_WARCRAFT_3",
            Message = "You need to have Warcraft 3 purchased."
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
