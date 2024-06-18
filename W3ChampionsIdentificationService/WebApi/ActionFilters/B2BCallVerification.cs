using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace W3ChampionsIdentificationService.WebApi.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class B2BVerification : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<B2BVerificationFilter>();
        }
    }
}
