using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using W3ChampionsIdentificationService.ServiceVerification;
using W3ChampionsStatisticService.WebApi.ExceptionFilters;

namespace W3ChampionsIdentificationService.WebApi.ActionFilters
{
    public class B2BVerificationFilter :IAsyncActionFilter

    {
        private readonly B2BVerificationService _service;

        public B2BVerificationFilter(B2BVerificationService service)
        {
            _service = service;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var token = GetToken(context.HttpContext.Request.Headers[HeaderNames.Authorization]);
            if (token != null && _service.ValidateToken(token))
            {
                await next.Invoke();
            }

            var unauthorizedResult = new UnauthorizedObjectResult(new ErrorResult("Auth missing."));
            context.Result = unauthorizedResult;
        }
        private static string GetToken(StringValues authorization)
        {
            if (!AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                return null;
            }
            return headerValue.Scheme != "Bearer"? null: headerValue.Parameter;
        }
    }
}
