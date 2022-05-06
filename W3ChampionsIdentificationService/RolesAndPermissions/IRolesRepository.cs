using System.Collections.Generic;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public interface IRolesRepository
    {
        public Task<List<Role>> GetAllRoles();
        public Task CreateRole(Role role);
        public Task DeleteRole(string roleName);
        public Task UpdateRole(Role role);
        public Task<List<Role>> GetRolesForUser(string battleTag);
        public Task GiveRolesToUser(string battleTag, List<string> roles);
        public Task RemoveRolesFromUser(string battleTag, List<string> roles);
    }
}
