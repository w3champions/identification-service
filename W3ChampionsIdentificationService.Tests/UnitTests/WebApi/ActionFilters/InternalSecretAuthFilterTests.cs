using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.WebApi.ActionFilters;

namespace W3ChampionsIdentificationService.Tests.UnitTests.WebApi.ActionFilters;

[TestFixture]
public class InternalSecretAuthFilterTests
{
    private const string Secret = "s3cret";

    private static InternalSecretAuthFilter CreateFilter(string configuredSecret)
    {
        var config = new Mock<IAppConfig>();
        config.SetupGet(c => c.IdentificationInternalApiSecret).Returns(configuredSecret);
        return new InternalSecretAuthFilter(config.Object);
    }

    private static async Task<(ActionExecutingContext Context, bool NextInvoked)> RunAsync(
        string configuredSecret, string headerValue)
    {
        var httpContext = new DefaultHttpContext();
        if (headerValue != null)
        {
            httpContext.Request.Headers[InternalSecretAuthFilter.HeaderName] = headerValue;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(
            actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());

        var nextInvoked = false;
        await CreateFilter(configuredSecret).OnActionExecutionAsync(context, () =>
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        });

        return (context, nextInvoked);
    }

    [Test]
    public async Task CorrectSecret_InvokesAction()
    {
        var (context, nextInvoked) = await RunAsync(Secret, Secret);

        Assert.That(nextInvoked, Is.True);
        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public async Task WrongSecret_Returns401_ActionNotInvoked()
    {
        var (context, nextInvoked) = await RunAsync(Secret, "nope");

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(nextInvoked, Is.False);
    }

    [Test]
    public async Task MissingHeader_Returns401()
    {
        var (context, nextInvoked) = await RunAsync(Secret, null);

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(nextInvoked, Is.False);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task SecretUnset_FailsClosed(string configuredSecret)
    {
        var (context, nextInvoked) = await RunAsync(configuredSecret, "anything");

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(nextInvoked, Is.False);
    }

    [Test]
    public async Task SecretUnset_EmptyHeader_StillFailsClosed()
    {
        var (context, nextInvoked) = await RunAsync(null, "");

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(nextInvoked, Is.False);
    }

    [Test]
    public async Task PrefixOfSecret_Returns401()
    {
        var (context, nextInvoked) = await RunAsync(Secret, "s3cre");

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(nextInvoked, Is.False);
    }

    [Test]
    public async Task SecretWithDifferentCase_Returns401()
    {
        var (context, nextInvoked) = await RunAsync(Secret, "S3CRET");

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(nextInvoked, Is.False);
    }
}
