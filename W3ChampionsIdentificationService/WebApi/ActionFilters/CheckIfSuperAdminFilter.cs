using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using W3ChampionsIdentificationService.W3CAuthentication.Contracts;
using W3ChampionsStatisticService.WebApi.ExceptionFilters;

namespace W3ChampionsIdentificationService.WebApi.ActionFilters
{
    public class CheckIfSuperAdminFilter : IAsyncActionFilter
    {
        private readonly IW3CAuthenticationService _authService;
        public CheckIfSuperAdminFilter(IW3CAuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var queryString = HttpUtility.ParseQueryString(context.HttpContext.Request.QueryString.Value);
            if (queryString.AllKeys.Contains("authorization"))
            {
                var auth = queryString["authorization"];
                var res = _authService.GetUserByToken(auth);
                if (
                    res != null
                    && !string.IsNullOrEmpty(res.BattleTag)
                    && res.IsSuperAdmin)
                {
                    context.ActionArguments["battleTag"] = res.BattleTag;
                    await next.Invoke();
                }
            }

            var unauthorizedResult = new UnauthorizedObjectResult(new ErrorResult("SuperAdmins only!"));
            context.Result = unauthorizedResult;
        }
    }
}