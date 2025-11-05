using api.GraphQL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services)
        {
            services.AddGraphQLServer()
             .ModifyPagingOptions(opt =>
             {
                 opt.MaxPageSize = 100;
                 opt.DefaultPageSize = 25;
                 opt.IncludeTotalCount = true;
             })
            .RegisterQueries()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .ModifyCostOptions(options =>
            {
                options.EnforceCostLimits = false;
                options.ApplyCostDefaults = true;
            });
            
           
            return services;
        }
    }
}
