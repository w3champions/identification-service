using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Authorization
{
    public interface IBlizzardAuthenticationService
    {
        Task<BlizzardUserInfo> GetUser(string bearer);
        Task<OAuthToken> GetToken(string code, string redirectUri);
    }
}