using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public interface IRoleRepository
    {
        Task UpsertRole(Role role);

        Task<Role> LoadRole(string roleId);
    }
}