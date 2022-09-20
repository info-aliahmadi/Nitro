using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nitro.Core.Domain.Auth;
using Nitro.Infrastructure.Data;
using Nitro.Kernel.Interfaces;
using Nitro.Kernel.Interfaces.Data;
using Nitro.Service.MessageSender;

namespace Nitro.Infrastructure.ServiceRegistrar
{
    public static class ServiceRegistrarStartup
    {
        public static void AddServices(this IServiceCollection services)
        {


            // services
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IEmailSender, MessageSender>();
            services.AddScoped<ISmsSender, MessageSender>();


        }
    }
}
