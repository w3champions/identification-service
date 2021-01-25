using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Twitch;

namespace W3ChampionsIdentificationService.W3CAuthentication
{

    [ApiController]
    [Route("api/oauth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IBlizzardAuthenticationService _blizzardAuthenticationService;
        private readonly ITwitchAuthenticationService _twitchAuthenticationService;

        private static readonly string JwtTokenSecret = Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET") ?? "secret";

        public AuthorizationController(
            IBlizzardAuthenticationService blizzardAuthenticationService,
            ITwitchAuthenticationService twitchAuthenticationService)
        {
            _blizzardAuthenticationService = blizzardAuthenticationService;
            _twitchAuthenticationService = twitchAuthenticationService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetBlizzardToken([FromQuery] string code, [FromQuery] string redirectUri)
        {
            var token = await _blizzardAuthenticationService.GetToken(code, redirectUri);
            if (token == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var userInfo = await _blizzardAuthenticationService.GetUser(token.access_token);
            if (userInfo == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var w3User = W3CUserAuthentication.Create(userInfo.battletag, JwtTokenSecret);

            return Ok(w3User);
        }

        [HttpGet("user-info")]
        public IActionResult GetUserInfo([FromQuery] string jwt)
        {
            var user = W3CUserAuthentication.FromJWT(jwt);
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