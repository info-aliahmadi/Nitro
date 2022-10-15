using Microsoft.Extensions.DependencyInjection;
using Nitro.Core.Interfaces.Cms;
using Nitro.Kernel.Interfaces;
using Nitro.Service.Cms;

namespace Nitro.Service
{
    public static class ServiceRegistrar
    {
        public static void AddAllServices(this IServiceCollection services)
        {
            // services
            services.AddScoped<IEmailSender, MessageSender.MessageSender>();
            services.AddScoped<ISmsSender, MessageSender.MessageSender>();
            services.AddScoped<IAuthorService, AuthorService>();


        }
    }
}
