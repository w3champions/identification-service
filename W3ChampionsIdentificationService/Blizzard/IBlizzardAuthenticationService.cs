using System.Collections.Generic;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Blizzard;

public interface IBlizzardAuthenticationService
{
    Task<BlizzardUserInfo> GetUser(string bearer, BnetRegion region);
    Task<OAuthToken> GetToken(string code, string redirectUri, BnetRegion region);

    Task<(List<BlizzardPlayableTitle> titles, AuthenticationError error)> GetPlayableTitles(OAuthToken token, BnetRegion region);
}
