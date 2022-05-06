using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using W3ChampionsIdentificationService.WebApi.ActionFilters;

namespace W3ChampionsStatisticService.WebApi.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckIfSuperAdmin : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<CheckIfSuperAdminFilter>();
        }
    }
}