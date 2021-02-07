using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Blizzard
{
    public interface IBlizzardAuthenticationService
    {
        Task<BlizzardUserInfo> GetUser(string bearer, BnetRegion region = BnetRegion.eu);
        Task<OAuthToken> GetToken(string code, string redirectUri, BnetRegion region = BnetRegion.eu);
    }
}