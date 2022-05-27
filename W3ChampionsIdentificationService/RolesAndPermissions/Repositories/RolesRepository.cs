using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class RolesRepository : MongoDbRepositoryBase, IRolesRepository
    {
        public RolesRepository(MongoClient mongoClient, IAppConfig appConfig) : base(mongoClient, appConfig)
        {
        }

        public async Task<List<Role>> GetAllRoles(Expression<Func<Role, bool>> expression = null, int ? limit = null, int? offset = null)
        {
            return await LoadAll(expression, limit, offset);
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
