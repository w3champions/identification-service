using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

public interface IRolesRepository
{
    public Task<List<Role>> GetAllRoles(Expression<Func<Role, bool>> expression = null, int? limit = null, int? offset = null);
    public Task<Role> GetRole(string id);
    public Task CreateRole(Role role);
    public Task UpdateRole(Role role);
    public Task DeleteRole(string roleName);
}
