using System.Collections.Generic;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts
{
    public interface IUsersRepository
    {
        public Task<User> GetUser(string id);
        public Task<User> GetUserByTag(string battleTag);
        public Task<List<User>> GetAllUsers(int? limit = null, int? offset = null);
        public Task CreateUser(User user);
        public Task UpdateUser(User user);
        public Task DeleteUser(string id);
    }
}
