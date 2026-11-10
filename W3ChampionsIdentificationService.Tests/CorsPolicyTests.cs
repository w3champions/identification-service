using Microsoft.AspNetCore.Cors.Infrastructure;
using NUnit.Framework;

namespace W3ChampionsIdentificationService.Tests;

/// <summary>
/// <see cref="Startup.ConfigureCors"/>: any origin may call the API, never with the visitor's credentials. The IdP
/// session cookie is only used on top-level navigations (authorize, handoff), which CORS does not govern, so a
/// cross-origin script must never be able to send it and read the answer.
/// </summary>
[TestFixture]
public class CorsPolicyTests
{
    [Test]
    public void AnyOriginMayCallWithoutCredentials()
    {
        var builder = new CorsPolicyBuilder();
        Startup.ConfigureCors(builder);
        var policy = builder.Build();

        Assert.That(policy.AllowAnyOrigin, Is.True);
        Assert.That(policy.AllowAnyHeader, Is.True);
        Assert.That(policy.AllowAnyMethod, Is.True);
        Assert.That(policy.SupportsCredentials, Is.False);
    }
}
