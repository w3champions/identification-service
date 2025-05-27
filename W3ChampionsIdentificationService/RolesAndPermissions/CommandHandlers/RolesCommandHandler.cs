using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using Serilog;


namespace W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;

public class RolesCommandHandler(
    IRolesRepository rolesRepository,
    RolesAndPermissionsValidator validator) : IRolesCommandHandler
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly RolesAndPermissionsValidator _validator = validator;

    public async Task CreateRole(Role role)
    {
        var existingRole = await _rolesRepository.GetRole(role.Id);
        if (existingRole != null)
        {
            Log.Error("Role with id: {RoleId} already exists", role.Id);
            throw new HttpException(409, $"Role with id: {role.Id} already exists");
        }

        _validator.ValidateRole(role);
        await _validator.ValidatePermissionList(role.Permissions);
        Log.Information("Creating role: {Role}", role);
        await _rolesRepository.CreateRole(role);
    }

    public async Task DeleteRole(string id)
    {
        var role = await _rolesRepository.GetRole(id);
        if (role == null)
        {
            Log.Error("Role to delete with id: {RoleId} not found", id);
            throw new HttpException(404, $"Role with id: '{id}' not found");
        }

        Log.Information("Deleting role: {Role}", role);
        await _rolesRepository.DeleteRole(id);
    }

    public async Task UpdateRole(Role role)
    {
        _validator.ValidateRole(role);
        await _validator.ValidatePermissionList(role.Permissions);
        Log.Information("Updating role: {Role}", role);
        await _rolesRepository.UpdateRole(role);
    }
}
