using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace W3ChampionsIdentificationService.WebApi.ActionFilters;

[AttributeUsage(AttributeTargets.Method)]
public class HasPermissionsPermission : Attribute, IFilterFactory
{
    public bool IsReusable => false;
    public string PermissionName { get; set; }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<HasPermissionsPermissionFilter>();
    }
}
