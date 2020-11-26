using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Authorization
{
    public class InjectActingPlayerFromAuthCodeFilter : IAsyncActionFilter {
        private readonly IW3CAuthenticationService _authService;

        public InjectActingPlayerFromAuthCodeFilter(IW3CAuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var queryString = HttpUtility.ParseQueryString(context.HttpContext.Request.QueryString.Value);
            if (queryString.AllKeys.Contains("authorization"))
            {
                var auth = queryString["authorization"];
                var res = await _authService.GetUserByToken(auth);

                var actingPlayerContent = context.ActionDescriptor.Parameters.FirstOrDefault(a => a.Name == "actingPlayer");
                if (actingPlayerContent != null)
                {
                    context.ActionArguments["actingPlayer"] = res.BattleTag;
                    await next.Invoke();
                }
            }

            var unauthorizedResult = new UnauthorizedObjectResult(new ErrorResult("Sorry H4ckerb0i"));
            context.Result = unauthorizedResult;
        }
    }
}