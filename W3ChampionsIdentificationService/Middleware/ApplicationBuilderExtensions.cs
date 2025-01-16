using Microsoft.AspNetCore.Builder;

namespace W3ChampionsIdentificationService.Middleware;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHttpException(this IApplicationBuilder application)
    {
        return application.UseMiddleware<HttpExceptionMiddleware>();
    }
}
