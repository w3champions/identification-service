using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Microsoft
{
    public interface IMicrosoftAuthenticationService
    {
        Task<MicrosoftUser> GetUser(string idToken);
        Task<string> GetIdToken(string code, string redirectUri);
    }
}