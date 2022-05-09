using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class RolesRepository : MongoDbRepositoryBase, IRolesRepository
    {
        public RolesRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public async Task<List<Role>> GetAllRoles(int? limit, int? offset)
        {
            return await LoadAll<Role>(null, limit, offset);
        }

        public async Task<Role> GetRole(string id)
        {
            return await LoadFirst<Role>(x => x.Id == id);
        }

        public async Task CreateRole(Role role)
        {
            await Insert(role);
        }

        public async Task UpdateRole(Role role)
        {
            await Upsert(role);
        }

        public async Task DeleteRole(string id)
        {
            await Delete<Role>(id);
        }
    }
}
