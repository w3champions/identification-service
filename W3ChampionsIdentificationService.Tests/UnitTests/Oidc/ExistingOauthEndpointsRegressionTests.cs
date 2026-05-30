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
    // ── key material (same format as JwtTests.cs) ─────────────────────────
    private const string PrivateKey =
        "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n";

    private const string PublicKey =
        "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA3l8idcfxcViOdQ4nZZwX\nsP6l42CTQmTyPT3jSOhFvm+YvK0bvoHw16ITYYg9CYni/aDzi8oulrI65cr8xFKg\ncojZnnbTDJk9YLLUznwxBtqub7MNF89t1+pfUgUp2H3zGzoDSl31A205mwBDRXOd\n5Zn97Fi8268Doy2/kIYKgycd7BaC+MqdecJ0KRZMpWbsA5QOewK4zmQyf6hlGLPp\nBNYkE7n2vg3RxeVsw1duEfD39Zh6kBAuTLTJDTxuDXlSuf8vH8NvPQ3ROTGl6cSH\nz8YOUz2em5L8wIYAW8W0tAamOTw9T2wEHEFpy2qpOuYWXtk3v0x3sfplBuzm8LE/\nDusuSyipoS0ZJJQGsOA/G2oaOFwRew5q9M+NxlpCFJEuSNHKi609W+FjX04sxovx\nuEjyp9RNeQ8BSeiad5kSXDLSs9Dintn8oulzil5pA+ccJ8PldJYRqrTjx+lB+STO\nnsgtg1esTDVXn6HOva857LVv2AN5sT8siXTBcXRXLjDxWWoI5N3xrf4Vbh+p6P0c\nWoXB7puyt3IKb68Rk0DcTC5WW1vh4neVslL8uhxwprS7J3NvBqY4ds/zfHj+3q2P\nvDEX2DOgXidjIQjlIdjF0S+FzZ8wJd3LEcPXjDhqefdOSQDdG2G7dqBrDKTDpEQt\n2+Rj5s0owdQbrCEuvFe92jECAwEAAQ==\n-----END PUBLIC KEY-----\n";

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

    // ── Group A — response-shape regression ───────────────────────────────

    [Test]
    public void GetUserInfo_ReturnsOkWithExpectedW3CUserAuthentication()
    {
        // Arrange — mint a real w3c JWT using the same key pair as JwtTests
        var permissions = new List<string> { nameof(EPermission.Permissions), nameof(EPermission.Moderation) };
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, permissions);

        // Set the env var BEFORE the static field on AuthorizationController is
        // initialised (this is the first access to the type in this test run).
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", PublicKey);

        var controller = new AuthorizationController(
            Mock.Of<IBlizzardAuthenticationService>(),
            Mock.Of<ITwitchAuthenticationService>(),
            Mock.Of<IMicrosoftAuthenticationService>(),
            Mock.Of<IUsersRepository>(),
            Mock.Of<IRolesRepository>(),
            Mock.Of<IPermissionsRepository>(),
            Mock.Of<IMicrosoftIdentityRepository>());

        // Act
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
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, permissions);

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
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, new List<string>());

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
    public void MintedJwt_Payload_ContainsExpectedClaims()
    {
        // Guard: all claims that downstream consumers depend on must be present
        var permissions = new List<string> { "Permissions" };
        var bnetId = 123456789L;
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, permissions, bnetId);

        using var doc = DecodePayload(auth.JWT);
        var root = doc.RootElement;

        Assert.IsTrue(root.TryGetProperty("battleTag", out _), "battleTag claim must be present");
        Assert.IsTrue(root.TryGetProperty("isAdmin", out _),   "isAdmin claim must be present");
        Assert.IsTrue(root.TryGetProperty("name", out _),      "name claim must be present");
        Assert.IsTrue(root.TryGetProperty("permissions", out _),"permissions claim must be present");
        Assert.IsTrue(root.TryGetProperty("bnetId", out _),    "bnetId claim must be present");
    }

    [Test]
    public void MintedJwt_Payload_AbsenceOfIssuanceAndAudienceClaims()
    {
        // Guard: the w3c JWT non-breaking invariant — the payload must NOT gain
        // iss, aud, or jti.  Adding these would change the signed bytes and
        // break every external consumer that validates the signature.
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, new List<string>());

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
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, new List<string>());

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
        var auth = W3CUserAuthentication.Create("TestPlayer#9999", PrivateKey, permissions);

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
