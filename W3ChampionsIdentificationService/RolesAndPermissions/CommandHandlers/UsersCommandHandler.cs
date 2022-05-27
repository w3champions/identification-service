using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers
{
    public class UsersCommandHandler : IUsersCommandHandler
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IRolesRepository _rolesRepository;
        private readonly RolesAndPermissionsValidator _validator;
        public UsersCommandHandler(
            IUsersRepository usersRepository,
            IRolesRepository rolesRepository,
            RolesAndPermissionsValidator validator)
        {
            _usersRepository = usersRepository;
            _rolesRepository = rolesRepository;
            _validator = validator;
        }

        public async Task CreateUser(UserDTO user)
        {
            await _validator.ValidateRoleListHttp(user.Roles);
            await _validator.ValidateUserDtoHttp(user);
            var allUsers = await _usersRepository.GetAllUsers();
            var roles = await _rolesRepository.GetAllRoles(x => user.Roles.Contains(x.Id));

            if (allUsers.Where(x => x.Id == user.BattleTag).Any())
            {
                throw new HttpException(409, "User already exists");
            }

            var distinctPermissions = roles
                .Select(x => x.Permissions)
                .SelectMany(y => y)
                .Distinct()
                .ToList();

            await _usersRepository.CreateUser(new User()
            {
                Id = user.BattleTag,
                Permissions = roles
                    .Select(x => x.Permissions)
                    .SelectMany(y => y)
                    .Distinct()
                    .ToList(),
            });
        }

        public async Task UpdateUser(UserDTO user)
        {   
            await _validator.ValidateUserDtoHttp(user, false);
            await _validator.ValidateRoleListHttp(user.Roles);
            var roles = await _rolesRepository.GetAllRoles(x => user.Roles.Contains(x.Id));

            await _usersRepository.UpdateUser(new User()
            {
                Id = user.BattleTag,
                Permissions = roles
                    .Select(x => x.Permissions)
                    .SelectMany(y => y)
                    .Distinct()
                    .ToList(),
            });
        }

        public async Task DeleteUser(string id)
        {
            var user = await _usersRepository.GetUser(id);
            if (user == null)
            {
                throw new HttpException(404, $"Role with id: ${id} not found");
            }

            await _rolesRepository.DeleteRole(id);
        }
    }
}
