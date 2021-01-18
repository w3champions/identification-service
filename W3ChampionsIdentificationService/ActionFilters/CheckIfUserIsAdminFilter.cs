using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using W3ChampionsIdentificationService.Authorization;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.ActionFilters
{
    public class CheckIfUserIsAdminFilter : IAsyncActionFilter
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IW3CAuthenticationService _authService;
        public CheckIfUserIsAdminFilter(IRoleRepository roleRepository, IW3CAuthenticationService authService)
        {
            _roleRepository = roleRepository;
            _authService = authService;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var queryString = HttpUtility.ParseQueryString(context.HttpContext.Request.QueryString.Value);
            if (queryString.AllKeys.Contains("authorization"))
            {
                var auth = queryString["authorization"];
                var user = await _authService.GetUserByToken(auth);
                var role = await _roleRepository.LoadRole("admin");
                if (
                    user != null
                    && !string.IsNullOrEmpty(user.BattleTag)
                    && role != null
                    && role.Members.Any(p => p.ToLower() == user.BattleTag.ToLower())
                    )
                {
                    context.ActionArguments["battleTag"] = user.BattleTag;
                    await next.Invoke();
                }
            }

            context.Result = new UnauthorizedObjectResult(new ErrorResult("Sorry H4ckerb0i"));
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CheckIfUserIsAdmin : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<CheckIfUserIsAdminFilter>();
        }
    }
}