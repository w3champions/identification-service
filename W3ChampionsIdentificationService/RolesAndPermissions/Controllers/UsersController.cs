using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(
    IUsersRepository usersRepository,
    IUsersCommandHandler usersCommandHandler) : ControllerBase
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUsersCommandHandler _usersCommandHandler = usersCommandHandler;

    [HttpGet]
    [HasPermissionsPermission]
    public async Task<IActionResult> GetAll([FromQuery] int? limit, [FromQuery] int? offset)
    {
        var roles = await _usersRepository.GetAllUsers(limit, offset);
        return Ok(roles);
    }

    [HttpGet("{tag}")]
    [HasPermissionsPermission]
    public async Task<IActionResult> Get([FromRoute] string tag)
    {
        var user = await _usersRepository.GetUser(tag);
        return Ok(user);
    }

    [HttpPost]
    [HasPermissionsPermission]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        await _usersCommandHandler.CreateUser(user);
        return Ok();
    }

    [HttpPut]
    [HasPermissionsPermission]
    public async Task<IActionResult> Update([FromBody] User user)
    {
        await _usersCommandHandler.UpdateUser(user);
        return Ok();
    }

    [HttpDelete]
    [HasPermissionsPermission]
    public async Task<IActionResult> Delete([FromQuery] string userId)
    {
        await _usersCommandHandler.DeleteUser(userId);
        return Ok();
    }
}
