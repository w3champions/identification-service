using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;

public class UsersCommandHandler(
    IUsersRepository usersRepository,
    RolesAndPermissionsValidator validator) : IUsersCommandHandler
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly RolesAndPermissionsValidator _validator = validator;

    public async Task CreateUser(User user)
    {
        await _validator.ValidateRoleList(user.Roles);
        await _validator.ValidateCreateUser(user);

        await _usersRepository.CreateUser(new User()
        {
            Id = user.Id,
            Roles = user.Roles,
        });
    }

    public async Task UpdateUser(User user)
    {
        await _validator.ValidateRoleList(user.Roles);
        await _validator.ValidateUpdateUser(user);

        await _usersRepository.UpdateUser(new User()
        {
            Id = user.Id,
            Roles = user.Roles,
        });
    }

    public async Task DeleteUser(string id)
    {
        var user = await _usersRepository.GetUser(id);
        if (user == null)
        {
            throw new HttpException(404, $"Role with id: '{id}' not found");
        }

        await _usersRepository.DeleteUser(id);
    }
}
