// ============================================================
// MIGRATION-SAFETY LOCK — DO NOT WEAKEN
//
// This test file pins the observable contract of the existing
// /api/oauth/* endpoints and the w3c JWT format after the
// OpenIddict 7.5.0 + Microsoft.IdentityModel.* 8.16.0 +
// MongoDB.Driver 3.6.0 upgrades that ship with PR1.
//
// The IdentityModel 6→8 bump switched JWT JSON serialization
// from Newtonsoft.Json to System.Text.Json.  The two test
// groups below guard against that change silently altering:
//
//   (A) RESPONSE-SHAPE: /api/oauth/user-info still returns
//       OkObjectResult<W3CUserAuthentication> with the
//       expected BattleTag / Name / IsAdmin values.
//
//   (B) TOKEN-PAYLOAD BYTE-COMPAT: the minted JWT's base64url
//       payload (the signed bytes external consumers verify)
//       must retain:
//         • "permissions"  → JSON Array  (not a string)
//         • "exp"          → JSON Number (numeric epoch, not ISO)
//         • "battleTag", "isAdmin", "name", "bnetId" present
//         • "iss", "aud", "jti" ABSENT  (w3c JWT non-breaking
//           invariant — adding these would break every consumer
//           that validates the signature over the exact payload)
//         • "iat", "nbf"   ABSENT  (JwtSecurityToken without an
//           explicit issuer/audience does not emit these; locking
//           prevents a future IdentityModel release from silently
//           adding them to the signed payload)
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

/// <summary>
/// Regression guard for the existing /api/oauth/* JWT issuance path.
/// See the file header for the full rationale.
/// </summary>
[TestFixture]
public class ExistingOauthEndpointsRegressionTests
{
    // ── helper ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Decodes the payload segment of a JWT and returns the parsed JSON document.
    /// The caller is responsible for disposing the document.
    /// </summary>
    private static JsonDocument DecodePayload(string jwt)
    {
        var parts = jwt.Split('.');
        Assert.AreEqual(3, parts.Length, "Expected a three-part JWT (header.payload.signature)");
        // Base64url → standard base64
        var base64 = parts[1].Replace('-', '+').Replace('_', '/');
        // Pad to 4-byte boundary
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        var bytes = Convert.FromBase64String(base64);
        var json = Encoding.UTF8.GetString(bytes);
        return JsonDocument.Parse(json);
    }

    // ── env hygiene ────────────────────────────────────────────────────────

    private string _originalJwtPublicKey;

    [SetUp]
    public void SetUp()
    {
        // Capture and override JWT_PUBLIC_KEY so GetUserInfo validates against
        // our test key pair.  NOTE: AuthorizationController.JwtPublicKey is a
        // static readonly field captured on first type access — the ordering
        // assumption (env set before the type is first touched) still holds.
        // Restoring on teardown is hygiene so the override does not leak to
        // other tests / fixtures in the same process.
        _originalJwtPublicKey = Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY");
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", TestJwtKeys.PublicKey);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", _originalJwtPublicKey);
    }

    // ── Group A — response-shape regression ───────────────────────────────

    [Test]
    public void GetUserInfo_ReturnsOkWithExpectedW3CUserAuthentication()
    {
        // Arrange — mint a real w3c JWT using the same key pair as JwtTests
        var permissions = new List<string> { nameof(EPermission.Permissions), nameof(EPermission.Moderation) };
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, permissions);

        var controller = new AuthorizationController(
            Mock.Of<IBlizzardAuthenticationService>(),
            Mock.Of<ITwitchAuthenticationService>(),
            Mock.Of<IMicrosoftAuthenticationService>(),
            Mock.Of<IUsersRepository>(),
            Mock.Of<IRolesRepository>(),
            Mock.Of<IPermissionsRepository>(),
            Mock.Of<IMicrosoftIdentityRepository>());
        // GetUserInfo reads the Authorization header, so it needs an HttpContext.
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act — legacy ?jwt= query path (the header path is asserted against the same shape in UserInfoBearerHeaderTests)
        var result = controller.GetUserInfo(auth.JWT);

        // Assert — response shape
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected 200 OK, got {0}", result?.GetType().Name);

        var body = ok.Value as W3CUserAuthentication;
        Assert.IsNotNull(body, "Response body must be W3CUserAuthentication");
        Assert.AreEqual("TestPlayer#9999", body.BattleTag, "BattleTag must round-trip unchanged");
        Assert.AreEqual("TestPlayer", body.Name, "Name must be the portion before '#'");
        Assert.IsTrue(body.IsAdmin, "IsAdmin must be true when permissions are non-empty");
    }

    // ── Group B — token payload byte-compatibility ─────────────────────────

    [Test]
    public void MintedJwt_Payload_PermissionsIsJsonArray()
    {
        // Guard: IdentityModel 8.x must NOT serialise the array claim as a string
        var permissions = new List<string> { "Permissions", "Moderation" };
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, permissions);

        using var doc = DecodePayload(auth.JWT);
        Assert.IsTrue(
            doc.RootElement.TryGetProperty("permissions", out var permEl),
            "JWT payload must contain 'permissions' claim");
        Assert.AreEqual(
            JsonValueKind.Array, permEl.ValueKind,
            "permissions must be a JSON array — not a serialised string; " +
            "JsonClaimValueTypes.JsonArray must survive the IdentityModel 6→8 upgrade");
    }

    [Test]
    public void MintedJwt_Payload_ExpIsNumericEpoch()
    {
        // Guard: exp must be a JSON number (Unix epoch), not an ISO-8601 string
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, new List<string>());

        using var doc = DecodePayload(auth.JWT);
        Assert.IsTrue(
            doc.RootElement.TryGetProperty("exp", out var expEl),
            "JWT payload must contain 'exp' claim");
        Assert.AreEqual(
            JsonValueKind.Number, expEl.ValueKind,
            "exp must be a numeric epoch, not an ISO string; " +
            "this guards against IdentityModel changing the serialisation format");
    }

    [Test]
    public void MintedJwt_Payload_IsAdminIsJsonString()
    {
        // Guard: today the code emits isAdmin via isAdmin.ToString() with no
        // JsonClaimValueTypes annotation, so it serialises as a JSON STRING
        // ("True"/"False") — NOT a JSON boolean.  Consumers parse it with
        // Boolean.Parse on the string value, so this shape must not silently
        // flip to a JSON boolean if a future IdentityModel/serialiser changes
        // claim handling.
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, new List<string> { "Permissions" });

        using var doc = DecodePayload(auth.JWT);
        Assert.IsTrue(
            doc.RootElement.TryGetProperty("isAdmin", out var isAdminEl),
            "JWT payload must contain 'isAdmin' claim");
        Assert.AreEqual(
            JsonValueKind.String, isAdminEl.ValueKind,
            "isAdmin must be a JSON string ('True'/'False'), not a JSON boolean; " +
            "this locks the current byte-compat shape consumers depend on");
    }

    [Test]
    public void MintedJwt_Payload_ContainsExpectedClaims()
    {
        // Guard: all claims that downstream consumers depend on must be present
        var permissions = new List<string> { "Permissions" };
        var bnetId = 123456789L;
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, permissions, bnetId);

        using var doc = DecodePayload(auth.JWT);
        var root = doc.RootElement;

        Assert.IsTrue(root.TryGetProperty("battleTag", out _), "battleTag claim must be present");
        Assert.IsTrue(root.TryGetProperty("isAdmin", out _), "isAdmin claim must be present");
        Assert.IsTrue(root.TryGetProperty("name", out _), "name claim must be present");
        Assert.IsTrue(root.TryGetProperty("permissions", out _), "permissions claim must be present");
        Assert.IsTrue(root.TryGetProperty("bnetId", out _), "bnetId claim must be present");
    }

    [Test]
    public void MintedJwt_Payload_AbsenceOfIssuanceAndAudienceClaims()
    {
        // Guard: the w3c JWT non-breaking invariant — the payload must NOT gain
        // iss, aud, or jti.  Adding these would change the signed bytes and
        // break every external consumer that validates the signature.
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, new List<string>());

        using var doc = DecodePayload(auth.JWT);
        var root = doc.RootElement;

        Assert.IsFalse(root.TryGetProperty("iss", out _), "iss must NOT be present in the w3c JWT");
        Assert.IsFalse(root.TryGetProperty("aud", out _), "aud must NOT be present in the w3c JWT");
        Assert.IsFalse(root.TryGetProperty("jti", out _), "jti must NOT be present in the w3c JWT");
    }

    [Test]
    public void MintedJwt_Payload_AbsenceOfIatAndNbf()
    {
        // Guard: JwtSecurityToken (without an explicit issuer/audience) does not
        // auto-inject iat or nbf.  Locking this prevents a future IdentityModel
        // release from silently altering the signed payload bytes.
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, new List<string>());

        using var doc = DecodePayload(auth.JWT);
        var root = doc.RootElement;

        Assert.IsFalse(root.TryGetProperty("iat", out _), "iat must NOT be present (auto-injected by IdentityModel would change signed bytes)");
        Assert.IsFalse(root.TryGetProperty("nbf", out _), "nbf must NOT be present (auto-injected by IdentityModel would change signed bytes)");
    }

    [Test]
    public void MintedJwt_Payload_PermissionsValuesMatchInput()
    {
        // Guard: verify the actual array element values round-trip correctly
        var permissions = new List<string> { "Permissions", "Moderation", "Tournaments" };
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, permissions);

        using var doc = DecodePayload(auth.JWT);
        doc.RootElement.TryGetProperty("permissions", out var permEl);

        var actual = new List<string>();
        foreach (var element in permEl.EnumerateArray())
        {
            actual.Add(element.GetString());
        }

        CollectionAssert.AreEquivalent(permissions, actual,
            "Permission array elements must round-trip unchanged through the JWT payload");
    }
}
