using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Identity;

namespace W3ChampionsIdentificationService.Identity.Contracts;

public interface IMicrosoftIdentityRepository
{
    public Task CreateIndex();
    public Task<MicrosoftIdentity> GetIdentity(string id);
    public Task<MicrosoftIdentity> GetIdentityByBattleTag(string battleTag);
    public Task LinkBattleTag(string id, string battleTag);
}
