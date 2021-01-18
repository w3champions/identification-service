using System.Threading.Tasks;
using MongoDB.Driver;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class RoleRepository : MongoDbRepositoryBase, IRoleRepository
    {
        public RoleRepository(MongoClient mongoClient) : base(mongoClient)
        {
            
        }
        public Task UpsertRole(Role role)
        {
            return Upsert(role, r => r.Id == role.Id);
        }
        public Task<Role> LoadRole(string roleId)
        {
            return LoadFirst<Role>(r => r.Id == roleId);
        }
    }
}