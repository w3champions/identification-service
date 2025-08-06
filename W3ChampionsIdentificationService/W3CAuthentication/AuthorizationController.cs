using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;

namespace W3ChampionsIdentificationService.W3CAuthentication;

[ApiController]
[Route("api/oauth")]
public class AuthorizationController(
    IBlizzardAuthenticationService blizzardAuthenticationService,
    ITwitchAuthenticationService twitchAuthenticationService,
    IMicrosoftAuthenticationService microsoftAuthenticationService,
    IUsersRepository usersRepository,
    IRolesRepository rolesRepository,
    IPermissionsRepository permissionsRepository,
    IMicrosoftIdentityRepository microsoftIdentityRepository) : ControllerBase
{
    private const bool ENFORCE_PLAYABLE_TITLES_SCOPE = false;
    private const bool ENFORCE_WARCRAFT_3_OWNERSHIP = false;

    private readonly IBlizzardAuthenticationService _blizzardAuthenticationService = blizzardAuthenticationService;
    private readonly ITwitchAuthenticationService _twitchAuthenticationService = twitchAuthenticationService;
    private readonly IMicrosoftAuthenticationService _microsoftAuthenticationService = microsoftAuthenticationService;
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IPermissionsRepository _permissionsRepository = permissionsRepository;
    private readonly IMicrosoftIdentityRepository _microsoftIdentityRepository = microsoftIdentityRepository;

    private static readonly string JwtPrivateKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY") ?? "");
    private static readonly string JwtPublicKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY") ?? "");

    [HttpGet("token")]
    public async Task<IActionResult> GetBlizzardToken(
        [FromQuery] string code,
        [FromQuery] string redirectUri,
        [FromQuery] BnetRegion region)
    {
        var token = await _blizzardAuthenticationService.GetToken(code, redirectUri, region);
        if (token == null)
        {
            return Unauthorized("Unable to get token");
        }

        var userInfo = await _blizzardAuthenticationService.GetUser(token.access_token, region);
        if (userInfo == null)
        {
            return Unauthorized("Unable to get user info");
        }

        var (titles, playableTitleError) = await _blizzardAuthenticationService.GetPlayableTitles(token, region);
        if (playableTitleError != null)
        {
            Log.Error("Unable to get playable titles for {BattleTag}: {Error}", userInfo.battletag, playableTitleError.Message);
            if (ENFORCE_PLAYABLE_TITLES_SCOPE)
            {
                return Unauthorized(playableTitleError);
            }
            else
            {
                titles = [];
            }
        }

        if (!titles.Any(t => t == BlizzardPlayableTitle.Warcraft3Reforged || t == BlizzardPlayableTitle.Warcraft3ReignOfChaos))
        {
            Log.Warning("User {BattleTag} does not have Warcraft 3 in their Battle.Net account - titles: {Titles}", userInfo.battletag, string.Join(", ", titles.Select(t => t.ToString())));
            if (ENFORCE_WARCRAFT_3_OWNERSHIP)
            {
                var error = PlayableTitleError.MissingWarcraft3();
                return Unauthorized(error);
            }
        }

        var user = await _usersRepository.GetUser(userInfo.battletag);
        var permissions = await _permissionsRepository.GetPermissionsForAdmin(userInfo.battletag);

        // Save user's Battle.net account id to the database
        if (string.IsNullOrEmpty(user?.BnetId))
        {
            await _usersRepository.UpdateUser(new User
            {
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

        var permissions = await _permissionsRepository.GetPermissionsForAdmin(userInfo.battleTag);

        var w3User = W3CUserAuthentication.Create(userInfo.battleTag, JwtPrivateKey, permissions);

        return Ok(w3User);
    }

    [HttpGet("user-info")]
    public IActionResult GetUserInfo([FromQuery] string jwt)
    {
        var user = W3CUserAuthentication.FromJWT(jwt, JwtPublicKey);
        return user != null ? Ok(user) : Unauthorized("Sorry Hackerboi");
    }

    [HttpGet("twitch")]
    public async Task<IActionResult> GetTwitchToken()
    {
        var token = await _twitchAuthenticationService.GetToken();
        return Ok(token);
    }
}
