using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class PermissionsRepository : MongoDbRepositoryBase, IPermissionsRepository
    {
        public PermissionsRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public async Task<List<Permission>> GetAllPermissions(int? limit, int? offset)
        {
            return await LoadAll<Permission>(null, limit, offset);
        }

        public async Task<Permission> GetPermission(string id)
        {
            return await LoadFirst<Permission>(x => x.Id == id);
        }

        public async Task CreatePermission(Permission permission)
        {
            await Insert(permission);
        }

        public async Task UpdatePermission(Permission permission)
        {
            await Upsert(permission);
        }

        public async Task DeletePermission(string id)
        {
            await Delete<Permission>(id);
        }
    }
}
