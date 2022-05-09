using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts
{
    public interface IRolesCommandHandler
    {
        public Task CreateRole(Role role);
        public Task UpdateRole(Role role);
        public Task DeleteRole(string id);
    }
}
