using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using W3ChampionsIdentificationService.Middleware;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class RolesRepository : MongoDbRepositoryBase, IRolesRepository
    {
        public RolesRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public Task<List<Role>> GetAllRoles()
        {
            return LoadAll<Role>();
        }

        public async Task<List<Role>> LoadByName(List<string> names)
        {
            var maps = CreateCollection<Role>();
            return await maps
                .Aggregate()
                .Match(x => names.Contains(x.Name))
                .ToListAsync();
        }

        public async Task<User> LoadByUser(string battleTag)
        {
            var maps = CreateCollection<User>();
            return await maps
                .Find(x => x.BattleTag == battleTag)
                .SingleOrDefaultAsync();
        }

        public async Task CreateRole(Role role)
        {
            var allRoles = await LoadAll<Role>();
            if (!allRoles.Select(x => x.Name).Contains(role.Name))
            {
                await Insert(role);
            }
        }

        public async Task DeleteRole(string roleName)
        {
            await Delete<Role>(x => x.Name == roleName);
        }

        public async Task<List<Role>> GetRolesForUser(string battleTag)
        {
            var user = await LoadByUser(battleTag);
            return user.Roles;
        }

        public async Task GiveRolesToUser(string battleTag, List<string> roles)
        {
            var user = await LoadByUser(battleTag);
            if (user == null)
            {
                throw new Exception("User doesn't exist");
            }

            var allRoles = await LoadAll<Role>();
            roles.ForEach(x =>
            {
                if (!user.Roles.Select(x => x.Name).Contains(x) && allRoles.Exists(y => y.Name == x))
                {
                    user.Roles.Add(allRoles.Find(y => y.Name == x));
                }
            });
            await Upsert(user);
        }

        public async Task RemoveRolesFromUser(string battleTag, List<string> roles)
        {
            var user = await LoadByUser(battleTag);
            if (user == null)
            {
                throw new HttpException(404, "User not found");
            }

            var allRoles = await LoadAll<Role>();
            roles.ForEach(x =>
            {
                if (user.Roles.Select(y => y.Name).Contains(x) && allRoles.Exists(y => y.Name == x))
                {
                    user.Roles.Remove(allRoles.Find(y => y.Name == x));
                }
            });
            await Upsert(user);
        }

        public Task UpdateRole(Role role)
        {
            return Task.CompletedTask;
        }
    }
}
