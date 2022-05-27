using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts
{
    public interface IUsersCommandHandler
    {
        public Task CreateUser(UserDTO user);
        public Task UpdateUser(UserDTO user);
        public Task DeleteUser(string id);
    }
}
