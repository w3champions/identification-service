namespace W3ChampionsIdentificationService.Blizzard;

public class GetPlayableTitlesResponse : IBlizzardApiResponse
{
    public BlizzardApiError error { get; set; }
    public int[] titleIds { get; set; }
}
