using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Middleware;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController(
    IPermissionsRepository permissionsRepository,
    IPermissionsCommandHandler permissionsCommandHandler) : ControllerBase
{
    private readonly IPermissionsRepository _permissionsRepository = permissionsRepository;
    private readonly IPermissionsCommandHandler _permissionsCommandHandler = permissionsCommandHandler;

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
        try
        {
            await _permissionsCommandHandler.CreatePermission(permission);
        }
        catch (HttpException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
        return Ok();
    }

    [HttpDelete]
    [HasPermissionsPermission]
    public async Task<IActionResult> Delete([FromQuery] string id)
    {
        try
        {
            await _permissionsCommandHandler.DeletePermission(id);
        }
        catch (HttpException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
        return Ok();
    }

    [HttpPut]
    [HasPermissionsPermission]
    public async Task<IActionResult> Update([FromBody] Permission permission)
    {
        try
        {
            await _permissionsCommandHandler.UpdatePermission(permission);
        }
        catch (HttpException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
        return Ok();
    }
}
