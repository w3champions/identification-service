using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.WebApi.ActionFilters;

/// <summary>
/// Guards service-to-service endpoints with a shared secret sent in the
/// <see cref="HeaderName"/> header. Fails closed: an unset/blank configured secret,
/// a missing header or a mismatch all yield a bare 401.
/// </summary>
public class InternalSecretAuthFilter(IAppConfig appConfig) : IAsyncActionFilter
{
    public const string HeaderName = "x-identification-internal-secret";

    private readonly IAppConfig _appConfig = appConfig;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var expected = _appConfig.IdentificationInternalApiSecret;
        if (string.IsNullOrWhiteSpace(expected)
            || !context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !SecretsMatch(expected, provided.ToString()))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }

    private static bool SecretsMatch(string expected, string provided)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(provided));
    }
}
