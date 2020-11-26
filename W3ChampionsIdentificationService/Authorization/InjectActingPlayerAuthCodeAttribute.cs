using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace W3ChampionsIdentificationService.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InjectActingPlayerAuthCodeAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<InjectActingPlayerFromAuthCodeFilter>();
        }
    }
}