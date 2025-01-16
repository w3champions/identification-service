using Microsoft.AspNetCore.Builder;
using W3ChampionsIdentificationService.Middleware;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHttpException(this IApplicationBuilder application)
    {
        return application.UseMiddleware<HttpExceptionMiddleware>();
    }
}
