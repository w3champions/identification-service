using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Twitch;

public interface ITwitchAuthenticationService
{
    Task<OAuthToken> GetToken();
}
