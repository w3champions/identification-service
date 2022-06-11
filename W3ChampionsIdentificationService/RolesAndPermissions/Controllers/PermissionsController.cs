using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{

    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionsRepository _permissionsRepository;
        private readonly IPermissionsCommandHandler _permissionsCommandHandler;
        public PermissionsController(
            IPermissionsRepository permissionsRepository,
            IPermissionsCommandHandler permissionsCommandHandler)
        {
            _permissionsRepository = permissionsRepository;
            _permissionsCommandHandler = permissionsCommandHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? limit, [FromQuery] int? offset)
        {
            var permissions = await _permissionsRepository.GetAllPermissions(limit, offset);
            return Ok(permissions);
        }

        [HttpPost]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Create([FromBody] Permission permission)
        {
            await _permissionsCommandHandler.CreatePermission(permission);
            return Ok();
        }

        [HttpDelete]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            await _permissionsCommandHandler.DeletePermission(id);
            return Ok();
        }

        [HttpPut]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Update([FromBody] Permission permission)
        {
            await _permissionsCommandHandler.UpdatePermission(permission);
            return Ok();
        }
    }
}