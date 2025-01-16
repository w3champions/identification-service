using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Identity.Contracts;

public interface IMicrosoftIdentityRepository
{
    public Task CreateIndex();
    public Task<MicrosoftIdentity> GetIdentity(string id);
    public Task<MicrosoftIdentity> GetIdentityByBattleTag(string battleTag);
    public Task LinkBattleTag(string id, string battleTag);
}
