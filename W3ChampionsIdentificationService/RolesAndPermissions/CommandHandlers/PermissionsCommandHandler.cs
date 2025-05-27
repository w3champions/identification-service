using System.Linq;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using Serilog;

namespace W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;

public class PermissionsCommandHandler(
    IPermissionsRepository permissionsRepository,
    RolesAndPermissionsValidator validator) : IPermissionsCommandHandler
{
    private readonly IPermissionsRepository _permissionsRepository = permissionsRepository;
    private readonly RolesAndPermissionsValidator _validator = validator;

    public async Task CreatePermission(Permission permission)
    {
        var allPermissions = await _permissionsRepository.GetAllPermissions();
        if (allPermissions.Select(x => x.Id).Contains(permission.Id))
        {
            Log.Error("Permission with id: {PermissionId} already exists", permission.Id);
            throw new HttpException(409, $"Permission with id: {permission.Id} already exists");
        }

        _validator.ValidatePermission(permission);
        Log.Information("Creating permission: {Permission}", permission);
        await _permissionsRepository.CreatePermission(permission);
    }

    public async Task DeletePermission(string id)
    {
        var permission = await _permissionsRepository.GetPermission(id);
        if (permission == null)
        {
            Log.Error("Permission with id: {PermissionId} not found", id);
            throw new HttpException(404, $"Permission with id: ${id} not found");
        }

        Log.Information("Deleting permission: {Permission}", permission);
        await _permissionsRepository.DeletePermission(id);
    }

    public async Task UpdatePermission(Permission permission)
    {
        Log.Information("Updating permission: {Permission}", permission);
        _validator.ValidatePermission(permission);
        await _permissionsRepository.UpdatePermission(permission);
    }
}
