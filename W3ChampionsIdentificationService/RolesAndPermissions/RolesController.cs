using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.WebApi.ActionFilters;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{

    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRolesRepository rolesRepository;
        public RolesController(IRolesRepository _rolesRepository)
        {
            rolesRepository = _rolesRepository;
        }

        // Roles

        [HttpGet]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await rolesRepository.GetAllRoles();
            return Ok(roles);
        }

        [HttpPost]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> PostNewRole([FromBody] Role role)
        {
            await rolesRepository.CreateRole(role);
            return Ok();
        }

        [HttpDelete("{roleName}")]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> DeleteRoles([FromRoute] string roleName)
        {
            await rolesRepository.DeleteRole(roleName);
            return Ok();
        }

        // Users

        [HttpGet("{battleTag}")]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> GetUsersRoles([FromRoute] string battleTag)
        {
            var roles = await rolesRepository.GetRolesForUser(battleTag);
            return Ok(roles);
        }

        // TODO
        // POST AddUserWithRoles()
        // PUT AddRolesToUser()
        // DELETE RemoveRolesFromUser()
    }
}