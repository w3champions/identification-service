using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

public interface IPermissionsCommandHandler
{
    public Task CreatePermission(Permission role);
    public Task UpdatePermission(Permission role);
    public Task DeletePermission(string id);
}
