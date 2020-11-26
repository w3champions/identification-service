using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Authorization
{
    public interface ITwitchAuthenticationService
    {
        Task<OAuthToken> GetToken();
    }
}