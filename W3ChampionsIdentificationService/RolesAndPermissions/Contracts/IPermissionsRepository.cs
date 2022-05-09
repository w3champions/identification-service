using System.Collections.Generic;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts
{
    public interface IPermissionsRepository
    {
        public Task<List<Permission>> GetAllPermissions(int? limit = null, int? offset = null);
        public Task<Permission> GetPermission(string id);
        public Task CreatePermission(Permission permission);
        public Task UpdatePermission(Permission permission);
        public Task DeletePermission(string id);
    }
}
