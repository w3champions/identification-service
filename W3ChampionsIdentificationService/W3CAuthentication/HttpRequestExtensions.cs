using System;
using Microsoft.AspNetCore.Http;

namespace W3ChampionsIdentificationService.W3CAuthentication;

public static class HttpRequestExtensions
{
    private const string BearerScheme = "Bearer";

    /// <summary>
    /// Returns the token of an "Authorization: Bearer &lt;token&gt;" header, or null if absent/malformed.
    /// The scheme is matched case-insensitively and surrounding whitespace is trimmed.
    /// </summary>
    public static string GetBearerToken(this HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString().Trim();
        if (!header.StartsWith(BearerScheme + " ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = header[BearerScheme.Length..].Trim();
        return token.Length > 0 ? token : null;
    }

    /// <summary>
    /// Resolves the caller's JWT: the "Authorization: Bearer" header wins, otherwise <paramref name="legacyQueryToken"/>.
    /// A malformed or non-Bearer header counts as absent and falls back to the query value; a well-formed Bearer
    /// header always wins, even if its token turns out to be invalid (no fallback in that case).
    /// The query-string token is DEPRECATED and only kept for old launchers/websites/backends still in the wild:
    /// tokens in URLs leak into proxy/access logs, client error logs and browser history.
    /// Remove the fallback (and this method) once those clients are gone.
    /// </summary>
    public static string GetJwt(this HttpRequest request, string legacyQueryToken) =>
        request.GetBearerToken() ?? legacyQueryToken;
}
