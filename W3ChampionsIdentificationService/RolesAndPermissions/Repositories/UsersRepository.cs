using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class UsersRepository : MongoDbRepositoryBase, IUsersRepository
    {
        public UsersRepository(MongoClient mongoClient, IAppConfig appConfig) : base(mongoClient, appConfig)
        {
        }

        public async Task<User> GetUserByTag(string battleTag)
        {
            var maps = CreateCollection<User>();
            return await maps
                .Find(x => x.Id == battleTag)
                .SingleOrDefaultAsync();
        }

        public async Task<User> GetUser(string id)
        {
            return await LoadFirst<User>(id);
        }

        public async Task<List<User>> GetAllUsers(int? limit = 50, int? offset = 0)
        {
            return await LoadAll<User>(null, limit, offset);
        }

        public async Task CreateUser(User user)
        {
            await Insert(user);
        }

        public async Task UpdateUser(User user)
        {
            await Upsert(user);
        }

        public async Task DeleteUser(string id)
        {
            await Delete<User>(id);
        }
    }
}
