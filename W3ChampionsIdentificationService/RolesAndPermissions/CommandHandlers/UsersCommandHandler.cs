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

        public async Task CreateUser(User user)
        {
            var allUsers = await _usersRepository.GetAllUsers();
            if (allUsers.Where(x => x.BattleTag == user.BattleTag || x.Id == user.Id).Any())
            {
                throw new HttpException(409, "User already exists");
            }

            await _validator.ValidatePermissionListHttp(user.Roles);
            await _usersRepository.CreateUser(user);
        }

        public async Task UpdateUser(User user)
        {   
            _validator.ValidateUserHttp(user);
            await _validator.ValidatePermissionListHttp(user.Roles);
            await _usersRepository.UpdateUser(user);
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
