using System.Text.Json.Serialization;

namespace W3ChampionsIdentificationService.Blizzard;

public class PlayableTitleError
{
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    public static PlayableTitleError ApiCallFailed()
    {
        return new PlayableTitleError
        {
            ErrorCode = "PLAYABLE_TITLES_API_FAILED",
            Message = "Unable to get playable titles"
        };
    }

    public static PlayableTitleError MissingWarcraft3()
    {
        return new PlayableTitleError
        {
            ErrorCode = "MISSING_WARCRAFT_3",
            Message = "You need to have Warcraft 3 purchased."
        };
    }

    public static PlayableTitleError MissingPlayableTitlesScope()
    {
        return new PlayableTitleError
        {
            ErrorCode = "MISSING_PLAYABLE_TITLES_SCOPE",
            Message = "You need to grant the streaming.titles scope."
        };
    }
}
