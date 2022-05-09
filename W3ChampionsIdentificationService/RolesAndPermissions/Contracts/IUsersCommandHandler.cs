using System.Collections.Generic;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts
{
    public interface IUsersCommandHandler
    {
        public Task CreateUser(User user);
        public Task UpdateUser(User user);
        public Task DeleteUser(string id);
    }
}
