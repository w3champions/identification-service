using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController(
    IRolesRepository rolesRepository,
    IRolesCommandHandler rolesCommandHandler) : ControllerBase
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IRolesCommandHandler _rolesCommandHandler = rolesCommandHandler;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? limit, [FromQuery] int? offset)
    {
        var roles = await _rolesRepository.GetAllRoles(null, limit, offset);
        return Ok(roles);
    }

    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetOne([FromRoute] string roleId)
    {
        return Ok(await _rolesRepository.GetRole(roleId));
    }

    [HttpPost]
    [HasPermissionsPermission]
    public async Task<IActionResult> Create([FromBody] Role role)
    {
        await _rolesCommandHandler.CreateRole(role);
        return Ok();
    }

    [HttpDelete]
    [HasPermissionsPermission]
    public async Task<IActionResult> Delete([FromQuery] string roleId)
    {
        await _rolesCommandHandler.DeleteRole(roleId);
        return Ok();
    }

    [HttpPut]
    [HasPermissionsPermission]
    public async Task<IActionResult> Update([FromBody] Role role)
    {
        await _rolesCommandHandler.UpdateRole(role);
        return Ok();
    }
}
