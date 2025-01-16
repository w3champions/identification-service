using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

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
            throw new HttpException(409, $"Role with id: {role.Id} already exists");
        }

        _validator.ValidateRole(role);
        await _validator.ValidatePermissionList(role.Permissions);
        await _rolesRepository.CreateRole(role);
    }

    public async Task DeleteRole(string id)
    {
        var role = await _rolesRepository.GetRole(id);
        if (role == null)
        {
            throw new HttpException(404, $"Role with id: '{id}' not found");
        }

        await _rolesRepository.DeleteRole(id);
    }

    public async Task UpdateRole(Role role)
    {
        _validator.ValidateRole(role);
        await _validator.ValidatePermissionList(role.Permissions);
        await _rolesRepository.UpdateRole(role);
    }
}
