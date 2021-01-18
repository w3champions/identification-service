using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.ActionFilters;
using W3ChampionsIdentificationService.Authorization;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class CreateRoleDto
    {
        public string name { get; set; }
    }
    [ApiController]
    [Route("api")]
    public class EntitlementController : ControllerBase
    {
        private IRoleRepository _roleRepository;
        public EntitlementController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpPost("role")]
        [InjectActingPlayerAuthCode]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto inputRole, string actingPlayer)
        {
            var role = Role.Create(inputRole.name, actingPlayer);
            await _roleRepository.UpsertRole(role);
            return Ok(role);
        }

        [HttpGet("role/{roleId}")]
        [CheckIfUserIsAdmin]
        public async Task<IActionResult> GetRole(string roleId)
        {
            var role = await _roleRepository.LoadRole(roleId);

            return Ok(role);
        }
    }
}