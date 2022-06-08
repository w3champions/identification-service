using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{

    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IUsersCommandHandler _usersCommandHandler;
        public UsersController(
            IUsersRepository usersRepository,
            IUsersCommandHandler usersCommandHandler)
        {
            _usersRepository = usersRepository;
            _usersCommandHandler = usersCommandHandler;
        }

        [HttpGet]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> GetAll([FromQuery] int? limit, [FromQuery] int? offset)
        {
            var roles = await _usersRepository.GetAllUsers(limit, offset);
            return Ok(roles);
        }

        [HttpGet("{tag}")]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Get([FromRoute] string tag)
        {
            var user = await _usersRepository.GetUser(tag);
            return Ok(user);
        }

        [HttpPost]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            await _usersCommandHandler.CreateUser(user);
            return Ok();
        }

        [HttpPut]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            await _usersCommandHandler.UpdateUser(user);
            return Ok();
        }

        [HttpDelete]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Delete([FromQuery] string userId)
        {
            await _usersCommandHandler.DeleteUser(userId);
            return Ok();
        }
    }
}