using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.WebApi.ActionFilters;

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
        [HasPermissionsPermission]
        public async Task<IActionResult> GetAll([FromQuery] int? limit, [FromQuery] int? offset)
        {
            var permissions = await _permissionsRepository.GetAllPermissions(limit, offset);
            return Ok(permissions);
        }

        [HttpPost]
        [HasPermissionsPermission]
        public async Task<IActionResult> Create([FromBody] Permission permission)
        {
            try {
                await _permissionsCommandHandler.CreatePermission(permission);
            } catch (HttpException ex) {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            return Ok();
        }

        [HttpDelete]
        [HasPermissionsPermission]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try {
                await _permissionsCommandHandler.DeletePermission(id);
            } catch (HttpException ex) {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            return Ok();
        }

        [HttpPut]
        [HasPermissionsPermission]
        public async Task<IActionResult> Update([FromBody] Permission permission)
        {
            try {
                await _permissionsCommandHandler.UpdatePermission(permission);
            } catch (HttpException ex) {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            return Ok();
        }
        [HttpPost("checkPermission")]
        [B2BVerification]
        public async Task<IActionResult> CheckPermission([FromBody] string permission, string battleTag)
        {
            var result = new { hasPermission = await _permissionsRepository.CheckPermission(battleTag, permission)};
            return Ok(result);
        }
    }
}