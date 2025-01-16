using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;

namespace W3ChampionsIdentificationService.W3CAuthentication;

[ApiController]
[Route("api/identity")]
public class IdentityController(
    IBlizzardAuthenticationService blizzardAuthenticationService,
    IMicrosoftAuthenticationService microsoftAuthenticationService,
    IMicrosoftIdentityRepository microsoftIdentityRepository) : ControllerBase
{
    private readonly IBlizzardAuthenticationService _blizzardAuthenticationService = blizzardAuthenticationService;
    private readonly IMicrosoftAuthenticationService _microsoftAuthenticationService = microsoftAuthenticationService;
    private readonly IMicrosoftIdentityRepository _microsoftIdentityRepository = microsoftIdentityRepository;

    private static readonly string JwtPublicKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY") ?? "");

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
