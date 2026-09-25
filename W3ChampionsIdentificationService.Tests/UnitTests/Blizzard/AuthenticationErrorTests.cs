using System.Text.Json;
using NUnit.Framework;
using W3ChampionsIdentificationService.Blizzard;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Blizzard;

// ASP.NET Core MVC in this service serializes controller results (e.g. Unauthorized(error))
// with System.Text.Json — Startup.ConfigureServices calls services.AddControllers() with no
// AddNewtonsoftJson() output formatter registered, so the [JsonPropertyName]/[JsonIgnore]
// System.Text.Json attributes on AuthenticationError are the ones that actually apply.
[TestFixture]
public class AuthenticationErrorTests
{
    [Test]
    public void MissingWarcraft3_SerializesWithErrorCodeAndBattleTag()
    {
        var error = AuthenticationError.MissingWarcraft3("Foo#1234");

        var json = JsonSerializer.Serialize(error);
        using var doc = JsonDocument.Parse(json);

        Assert.AreEqual("MISSING_WARCRAFT_3", doc.RootElement.GetProperty("errorCode").GetString());
        Assert.AreEqual("Foo#1234", doc.RootElement.GetProperty("battleTag").GetString());
    }

    [Test]
    public void MissingPlayableTitlesScope_SerializesWithoutBattleTagKey()
    {
        var error = AuthenticationError.MissingPlayableTitlesScope();

        var json = JsonSerializer.Serialize(error);
        using var doc = JsonDocument.Parse(json);

        Assert.AreEqual("MISSING_PLAYABLE_TITLES_SCOPE", doc.RootElement.GetProperty("errorCode").GetString());
        Assert.IsFalse(doc.RootElement.TryGetProperty("battleTag", out _),
            "battleTag must be omitted entirely for error codes that don't set it, not emitted as null.");
    }
}
