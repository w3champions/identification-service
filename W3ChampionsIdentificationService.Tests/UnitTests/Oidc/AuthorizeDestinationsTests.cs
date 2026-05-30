using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using W3ChampionsIdentificationService.Oidc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

/// <summary>
/// Guards the claim/scope/destination ordering in OidcAuthorizeController.Authorize: scopes must be
/// set BEFORE destinations so the private scope claims (oi_scp) added by SetScopes receive the
/// access-token destination via the `_ => AccessToken` default. If destinations are assigned first,
/// the scope claims get no destination, are dropped from the access token, and userinfo (which reads
/// principal.GetScopes() off the access token) loses name/email.
///
/// This reproduces the exact principal-building sequence without an HTTP host (SetScopes /
/// SetDestinations / GetDestinations are pure ClaimsPrincipal extensions over the claim set).
/// </summary>
[TestFixture]
public class AuthorizeDestinationsTests
{
    private const string BattleTag = "Modmoto#2809";

    private static ClaimsPrincipal BuildPrincipal(IEnumerable<string> scopes)
    {
        var scopeList = scopes.ToList();
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);

        identity.AddClaim(Claims.Subject, BattleTag);
        if (scopeList.Contains(Scopes.Profile))
            identity.AddClaim(Claims.Name, BattleTag);
        if (scopeList.Contains(Scopes.Email))
        {
            identity.AddClaim(Claims.Email, OidcClaimMapper.BattleTagToSyntheticEmail(BattleTag));
            identity.AddClaim(new Claim(Claims.EmailVerified, "false", ClaimValueTypes.Boolean));
        }

        var principal = new ClaimsPrincipal(identity);
        // Same order as the controller: scopes first, then destinations over all claims.
        principal.SetScopes(scopeList);
        principal.SetDestinations(claim => claim.Type switch
        {
            Claims.Subject => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            Claims.Name => new[] { Destinations.IdentityToken },
            Claims.Email => new[] { Destinations.IdentityToken },
            Claims.EmailVerified => new[] { Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        });
        return principal;
    }

    [Test]
    public void ScopeClaim_CarriesAccessTokenDestination()
    {
        var principal = BuildPrincipal(new[] { Scopes.OpenId, Scopes.Profile, Scopes.Email });

        // The scope claims SetScopes adds must each route to the access token, so the access token
        // carries the granted scopes and userinfo can read them back.
        var scopeClaims = principal.Claims.Where(c => c.Value is Scopes.OpenId or Scopes.Profile or Scopes.Email).ToList();
        Assert.IsNotEmpty(scopeClaims, "Expected SetScopes to add scope claims to the principal.");
        foreach (var scopeClaim in scopeClaims)
        {
            Assert.Contains(Destinations.AccessToken, scopeClaim.GetDestinations().ToArray(),
                $"Scope claim '{scopeClaim.Value}' must carry the AccessToken destination.");
        }
    }

    [Test]
    public void GrantedScopes_AreReadableFromThePrincipal()
    {
        var principal = BuildPrincipal(new[] { Scopes.OpenId, Scopes.Profile, Scopes.Email });

        // This is exactly what OidcUserInfoController reads to gate name/email.
        var scopes = principal.GetScopes();
        Assert.Contains(Scopes.Profile, scopes.ToArray(), "profile scope must be readable from the access-token principal.");
        Assert.Contains(Scopes.Email, scopes.ToArray(), "email scope must be readable from the access-token principal.");
    }

    [Test]
    public void Subject_CarriesBothAccessAndIdentityTokenDestinations()
    {
        var principal = BuildPrincipal(new[] { Scopes.OpenId, Scopes.Profile });

        var sub = principal.Claims.Single(c => c.Type == Claims.Subject);
        var destinations = sub.GetDestinations().ToArray();
        Assert.Contains(Destinations.AccessToken, destinations, "sub must reach the access token.");
        Assert.Contains(Destinations.IdentityToken, destinations, "sub must reach the id token.");
    }

    [Test]
    public void NameAndEmail_StayIdentityTokenOnly()
    {
        var principal = BuildPrincipal(new[] { Scopes.OpenId, Scopes.Profile, Scopes.Email });

        var name = principal.Claims.Single(c => c.Type == Claims.Name);
        var email = principal.Claims.Single(c => c.Type == Claims.Email);

        CollectionAssert.AreEquivalent(new[] { Destinations.IdentityToken }, name.GetDestinations().ToArray());
        CollectionAssert.AreEquivalent(new[] { Destinations.IdentityToken }, email.GetDestinations().ToArray());
    }
}
