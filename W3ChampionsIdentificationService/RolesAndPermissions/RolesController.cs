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

        [HttpGet]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await rolesRepository.GetAllRoles();
            return Ok(roles);
        }

        [HttpGet("{battleTag}")]
        public async Task<IActionResult> GetUserInfo([FromRoute] string battleTag)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PostNewRole([FromBody] Role role)
        {
            await rolesRepository.CreateRole(role);
            return Ok();
        }

        [HttpPut]
        public IActionResult ChangeRoles([FromRoute] string battleTag, [FromBody] List<string> roles)
        {
            return Ok();
        }
    }
}