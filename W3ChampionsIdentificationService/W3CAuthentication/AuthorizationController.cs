using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;

namespace W3ChampionsIdentificationService.W3CAuthentication
{

    [ApiController]
    [Route("api/oauth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IBlizzardAuthenticationService _blizzardAuthenticationService;
        private readonly ITwitchAuthenticationService _twitchAuthenticationService;
        private readonly IMicrosoftAuthenticationService _microsoftAuthenticationService;
        private readonly IUsersRepository _usersRepository;
        private readonly IRolesRepository _rolesRepository;
        private readonly IMicrosoftIdentityRepository _microsoftIdentityRepository;

        private static readonly string JwtPrivateKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY") ?? "");
        private static readonly string JwtPublicKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY") ?? "");

        public AuthorizationController(
            IBlizzardAuthenticationService blizzardAuthenticationService,
            ITwitchAuthenticationService twitchAuthenticationService,
            IMicrosoftAuthenticationService microsoftAuthenticationService,
            IUsersRepository usersRepository,
            IRolesRepository rolesRepository,
            IMicrosoftIdentityRepository microsoftIdentityRepository)
        {
            _blizzardAuthenticationService = blizzardAuthenticationService;
            _twitchAuthenticationService = twitchAuthenticationService;
            _microsoftAuthenticationService = microsoftAuthenticationService;
            _usersRepository = usersRepository;
            _rolesRepository = rolesRepository;
            _microsoftIdentityRepository = microsoftIdentityRepository;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetBlizzardToken(
            [FromQuery] string code,
            [FromQuery] string redirectUri,
            [FromQuery] BnetRegion region)
        {
            var token = await _blizzardAuthenticationService.GetToken(code, redirectUri, region);
            if (token == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var userInfo = await _blizzardAuthenticationService.GetUser(token.access_token, region);
            if (userInfo == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var user = await _usersRepository.GetUser(userInfo.battletag);
            var roles = user?.Roles != null ? await _rolesRepository.GetAllRoles(x => user.Roles.Contains(x.Id)) : new List<Role>();
            var permissions = roles.Count > 0 ? roles.SelectMany(x => x.Permissions).Distinct().ToList() : new List<string>();

            // Save user's Battle.net account id to the database
            if (string.IsNullOrEmpty(user?.BnetId))
            {
                await _usersRepository.UpdateUser(new User { 
                    Id = userInfo.battletag,
                    BnetId = userInfo.id.ToString(),
                });
            }

            var w3User = W3CUserAuthentication.Create(userInfo.battletag, JwtPrivateKey, permissions, userInfo.id);

            return Ok(w3User);
        }

        [HttpGet("token-microsoft")]
        public async Task<IActionResult> GetMicrosoftToken(
            [FromQuery] string code,
            [FromQuery] string redirectUri)
        {
            var token = await _microsoftAuthenticationService.GetIdToken(code, redirectUri);
            if (token == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }
            var u = await _microsoftAuthenticationService.GetUser(token);

            var userInfo = await _microsoftIdentityRepository.GetIdentity(u.sub);
            if (userInfo == null)
            {
                return Unauthorized("Not Linked");
            }

            var user = await _usersRepository.GetUser(userInfo.battleTag);
            var roles = user != null ? await _rolesRepository.GetAllRoles(x => user.Roles.Contains(x.Id)) : new List<Role>();
            var permissions = roles.Count > 0 ? roles.SelectMany(x => x.Permissions).Distinct().ToList() : new List<string>();

            var w3User = W3CUserAuthentication.Create(userInfo.battleTag, JwtPrivateKey, permissions);

            return Ok(w3User);
        }

        [HttpGet("user-info")]
        public IActionResult GetUserInfo([FromQuery] string jwt)
        {
            var user = W3CUserAuthentication.FromJWT(jwt, JwtPublicKey);
            return user != null ? (IActionResult) Ok(user) : Unauthorized("Sorry Hackerboi");
        }

        [HttpGet("twitch")]
        public async Task<IActionResult> GetTwitchToken()
        {
            var token = await _twitchAuthenticationService.GetToken();
            return Ok(token);
        }
    }
}