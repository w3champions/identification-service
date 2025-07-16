namespace W3ChampionsIdentificationService.Blizzard;

public interface IBlizzardApiResponse
{
    public BlizzardApiError error { get; set; }
    public string accountId { get; set; }
    public bool IsError => error != null;
}
