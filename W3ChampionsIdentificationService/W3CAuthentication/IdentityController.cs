using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;

namespace W3ChampionsIdentificationService.W3CAuthentication
{

    [ApiController]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IBlizzardAuthenticationService _blizzardAuthenticationService;
        private readonly IMicrosoftAuthenticationService _microsoftAuthenticationService;
        private readonly IMicrosoftIdentityRepository _microsoftIdentityRepository;

        private static readonly string JwtPublicKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY") ?? "");

        public IdentityController(
            IBlizzardAuthenticationService blizzardAuthenticationService,
            IMicrosoftAuthenticationService microsoftAuthenticationService,
            IMicrosoftIdentityRepository microsoftIdentityRepository)
        {
            _blizzardAuthenticationService = blizzardAuthenticationService;
            _microsoftAuthenticationService = microsoftAuthenticationService;
            _microsoftIdentityRepository = microsoftIdentityRepository;
        }

        [HttpGet("microsoft-identity-linked")]
        public async Task<IActionResult> GetMicrosoftIdentity([FromQuery] string jwt)
        {
            var user = W3CUserAuthentication.FromJWT(jwt, JwtPublicKey);
            if (user == null)
            {
                return Unauthorized("Sorry Hackerboi");
            }
            var identity = await _microsoftIdentityRepository.GetIdentityByBattleTag(user.BattleTag);
            return (IActionResult)Ok(identity != null);
        }

        [HttpPost("link-microsoft-identity")]
        public async Task<IActionResult> LinkMicrosoftIdentity(
            [FromQuery] string jwt,
            [FromQuery] string code,
            [FromQuery] string redirectUri
            )
        {
            var user = W3CUserAuthentication.FromJWT(jwt, JwtPublicKey);
            if (user == null)
            {
                return Unauthorized("Sorry Hackerboi");
            }
            var token = await _microsoftAuthenticationService.GetIdToken(code, redirectUri);
            var u = await _microsoftAuthenticationService.GetUser(token);
            await _microsoftIdentityRepository.LinkBattleTag(u.sub, user.BattleTag);
            return Ok();
        }
    }
}