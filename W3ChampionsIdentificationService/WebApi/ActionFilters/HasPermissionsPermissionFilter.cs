using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using W3ChampionsIdentificationService.W3CAuthentication.Contracts;
using W3ChampionsStatisticService.WebApi.ExceptionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.RolesAndPermissions;

namespace W3ChampionsIdentificationService.WebApi.ActionFilters;

public class HasPermissionsPermissionFilter : IAsyncActionFilter
{
    private readonly IW3CAuthenticationService _authService;
    private readonly IPermissionsRepository _permissionsRepository;
    public HasPermissionsPermissionFilter(
        IW3CAuthenticationService authService,
        IPermissionsRepository permissionsRepository)
    {
        _authService = authService;
        _permissionsRepository = permissionsRepository;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var queryString = HttpUtility.ParseQueryString(context.HttpContext.Request.QueryString.Value);
        if (queryString.AllKeys.Contains("authorization"))
        {
            var auth = queryString["authorization"];
            var res = _authService.GetUserByToken(auth);
            if (res != null && !string.IsNullOrEmpty(res.BattleTag))
            {
                var permissions = await _permissionsRepository.GetPermissionsForAdmin(res.BattleTag);
                if (permissions.Contains(nameof(EPermission.Permissions)))
                {
                    context.ActionArguments["battleTag"] = res.BattleTag;
                    await next.Invoke();
                }
            }
        }

        var unauthorizedResult = new UnauthorizedObjectResult(new ErrorResult("Permissing missing."));
        context.Result = unauthorizedResult;
    }
}
