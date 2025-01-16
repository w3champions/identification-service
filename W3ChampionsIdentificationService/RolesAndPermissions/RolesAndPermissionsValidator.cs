using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions;

public class RolesAndPermissionsValidator
{
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IUsersRepository _usersrepostiory;
    public RolesAndPermissionsValidator(
        IPermissionsRepository permissionsRepository,
        IRolesRepository rolesRepository,
        IUsersRepository usersRepository)
    {
        _permissionsRepository = permissionsRepository;
        _rolesRepository = rolesRepository;
        _usersrepostiory = usersRepository;
    }

    public void ValidatePermission(Permission permission)
    {
        if (string.IsNullOrEmpty(permission.Id))
        {
            throw new HttpException(400, "Id cannot be null or empty");
        }

        if (string.IsNullOrEmpty(permission.Description))
        {
            throw new HttpException(400, "Description cannot be null or empty");
        }
    }

    public void ValidateRole(Role role)
    {

        if (string.IsNullOrEmpty(role.Id))
        {
            throw new HttpException(400, "Id cannot be null or empty");
        }

        if (string.IsNullOrEmpty(role.Description))
        {
            throw new HttpException(400, "Description cannot be null or empty");
        }
    }

    public async Task ValidateCreateUser(User user)
    {
        ValidateBattleTag(user.Id);

        var existingUser = await _usersrepostiory.GetUser(user.Id);
        if (existingUser != null)
        {
            throw new HttpException(409, $"User: {user.Id} already exists");
        }
    }

    public async Task ValidateUpdateUser(User user)
    {
        ValidateBattleTag(user.Id);

        var existingUser = await _usersrepostiory.GetUser(user.Id);
        if (existingUser == null)
        {
            throw new HttpException(409, $"User: {user.Id} does not exist");
        }
    }

    public async Task ValidatePermissionList(List<string> permissions)
    {
        var validPermissions = await _permissionsRepository.GetAllPermissions();
        var invalidPermissions = permissions.Except(validPermissions.Select(x => x.Id)).ToList();
        if (invalidPermissions.Count > 0)
        {
            throw new HttpException(404, $"Permissions: '{string.Join("','", invalidPermissions)}' do not exist");
        }
    }

    public void ValidateBattleTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            throw new HttpException(400, "BattleTag cannot be null or empty");
        }

        var battleTagPattern = "^[a-zA-Z1-9]*#[1-9]{4,5}";
        if (!Regex.Match(tag, battleTagPattern).Success)
        {
            throw new HttpException(400, "BattleTag is not valid");
        }
    }

    public async Task ValidateRoleList(List<string> roles)
    {
        if (roles == null || roles.Count == 0)
        {
            throw new HttpException(400, "A user cannot have no Roles");
        }

        if (roles.GroupBy(x => x).Any(y => y.Count() > 1))
        {
            throw new HttpException(400, "Cannot have duplicate Roles");
        }

        var validRoles = await _rolesRepository.GetAllRoles(x => roles.Contains(x.Id));
        var invalidRoles = roles.Except(validRoles.Select(x => x.Id)).ToList();

        if (invalidRoles.Count > 0)
        {
            throw new HttpException(404, $"Roles: '{string.Join("','", invalidRoles)}' do not exist");
        }
    }
}
