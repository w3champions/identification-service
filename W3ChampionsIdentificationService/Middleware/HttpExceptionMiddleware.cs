using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Middleware;

internal class HttpExceptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (HttpException httpException)
        {
            context.Response.StatusCode = httpException.StatusCode;
            var responseFeature = context.Features.Get<IHttpResponseFeature>();
            responseFeature.ReasonPhrase = httpException.Message;
        }
    }
}
