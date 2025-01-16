using MongoDB.Driver;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.Identity.Contracts;

namespace W3ChampionsIdentificationService.Identity.Repositories;

public class MicrosoftIdentityRepository : MongoDbRepositoryBase, IMicrosoftIdentityRepository
{
    public MicrosoftIdentityRepository(MongoClient mongoClient, IAppConfig appConfig) : base(mongoClient, appConfig)
    {
    }

    public async Task CreateIndex()
    {
        var mongoCollection = CreateCollection<MicrosoftIdentity>();
        var indexKeysDefinition = Builders<MicrosoftIdentity>.IndexKeys.Ascending(entity => entity.battleTag);
        await mongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<MicrosoftIdentity>(indexKeysDefinition));
    }

    public async Task<MicrosoftIdentity> GetIdentity(string id)
    {
        return await LoadFirst<MicrosoftIdentity>(x => x.Id == id);
    }

    public async Task<MicrosoftIdentity> GetIdentityByBattleTag(string battleTag)
    {
        return await LoadFirst<MicrosoftIdentity>(x => x.battleTag == battleTag);
    }

    public async Task LinkBattleTag(string id, string battleTag)
    {
        await Upsert(new MicrosoftIdentity
        {
            Id = id,
            battleTag = battleTag
        });
    }
}
