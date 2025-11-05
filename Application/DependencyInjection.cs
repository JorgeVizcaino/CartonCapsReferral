using Application.Interfaces;
using Application.Referrals.Mocks;
using Application.Referrals.Services;
using Application.User.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IReferralService, ReferralsServices>();
            services.AddScoped<ILinkServices, LinkServices>();
            services.AddSingleton<IFraudService, MockFraudService>();
            services.AddScoped<IUserServices, UserServices>();

            return services;
        }
    }

}

