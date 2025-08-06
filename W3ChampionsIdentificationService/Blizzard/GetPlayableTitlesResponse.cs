namespace W3ChampionsIdentificationService.Blizzard;

public class GetPlayableTitlesResponse : IBlizzardApiResponse
{
    public BlizzardApiError error { get; set; }
    public string accountId { get; set; }
    public string[] titleCodes { get; set; }
}
