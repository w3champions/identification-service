using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{
    public class RolesAndPermissionsValidator
    {
        private readonly IPermissionsRepository _permissionsRepository;
        public RolesAndPermissionsValidator(IPermissionsRepository permissionsRepository)
        {
            _permissionsRepository = permissionsRepository;
        }

        public void ValidatePermissionHttp(Permission permission)
        {
            if (string.IsNullOrEmpty(permission.Id))
            {
                throw new HttpException(400, "Id cannot be null or empty");
            }

            if (string.IsNullOrEmpty(permission.Name))
            {
                throw new HttpException(400, "Name cannot be null or empty");
            }

            if (string.IsNullOrEmpty(permission.Description))
            {
                throw new HttpException(400, "Description cannot be null or empty");
            }
        }

        public void ValidateRoleHttp(Role role)
        {

            if (string.IsNullOrEmpty(role.Id))
            {
                throw new HttpException(400, "Id cannot be null or empty");
            }

            if (string.IsNullOrEmpty(role.Name))
            {
                throw new HttpException(400, "Name cannot be null or empty");
            }

            if (string.IsNullOrEmpty(role.Description))
            {
                throw new HttpException(400, "Description cannot be null or empty");
            }
        }

        public void ValidateUserHttp(User user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                throw new HttpException(400, "Id cannot be null or empty");
            }

            if (string.IsNullOrEmpty(user.BattleTag))
            {
                throw new HttpException(400, "BattleTag cannot be null or empty");
            }

            if (string.IsNullOrEmpty(user.Id))
            {
                throw new HttpException(400, "Id cannot be null or empty");
            }

            var battleTagPattern = "/^[a-zA-Z1-9]*#[1-9]{4,5}/g";
            if (!Regex.Match(user.BattleTag, battleTagPattern).Success)
            {
                throw new HttpException(400, "BattleTag is not valid");
            }
        }

        public async Task ValidatePermissionListHttp(List<string> permissions)
        {
            var validPermissions = await _permissionsRepository.GetAllPermissions();
            var invalidPermissions = permissions.Except(validPermissions.Select(x => x.Name)).ToList();
            if (invalidPermissions.Count > 0)
            {
                throw new HttpException(409, $"Permissions: '{string.Join("','", invalidPermissions)}' do not exist");
            }
        }
    }
}
