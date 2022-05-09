using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers
{
    public class RolesCommandHandler : IRolesCommandHandler
    {
        private readonly IRolesRepository _rolesRepository;
        private readonly IPermissionsRepository _permissionsRepository;
        private readonly RolesAndPermissionsValidator _validator;
        public RolesCommandHandler(
            IRolesRepository rolesRepository,
            IPermissionsRepository permissionsRepository,
            RolesAndPermissionsValidator validator)
        {
            _rolesRepository = rolesRepository;
            _permissionsRepository = permissionsRepository;
            _validator = validator;
        }

        public async Task CreateRole(Role role)
        {
            var allRoles = await _rolesRepository.GetAllRoles();
            if (allRoles.Select(x => x.Id).Contains(role.Id))
            {
                throw new HttpException(409, $"Role with id: {role.Id} already exists");
            }

            if (allRoles.Select(x => x.Name).Contains(role.Name))
            {
                throw new HttpException(409, $"Role with name: '{role.Name}' already exists");
            }

            _validator.ValidateRoleHttp(role);
            await _validator.ValidatePermissionListHttp(role.Permissions);
            await _rolesRepository.CreateRole(role);
        }

        public async Task DeleteRole(string id)
        {
            var role = await _rolesRepository.GetRole(id);
            if (role == null)
            {
                throw new HttpException(404, $"Role with id: ${id} not found");
            }

            await _rolesRepository.DeleteRole(id);
        }

        public async Task UpdateRole(Role role)
        {
            _validator.ValidateRoleHttp(role);
            await _validator.ValidatePermissionListHttp(role.Permissions);
            await _rolesRepository.UpdateRole(role);
        }
    }
}
