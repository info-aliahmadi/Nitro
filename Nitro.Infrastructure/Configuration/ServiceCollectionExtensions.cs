using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Nitro.Infrastructure.Data;
using Nitro.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Nitro.Infrastructure.Cache;
using Nitro.Infrastructure.Log;

namespace Nitro.Infrastructure.Configuration
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add services to the application and configure service provider
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="builder">A builder for web applications and services</param>
        public static void ConfigureApplicationServices(this IServiceCollection services,
            WebApplicationBuilder builder)
        {
            builder.AddSerilogConfig();

            services.AddDbContextConfig(builder.Configuration);

            services.AddIdentityConfig();

            services.AddCacheProvider(builder.Configuration);

            services.AddControllerConfig();

            services.AddSwaggerGenConfig();


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddControllerConfig(this IServiceCollection services)
        {
            services.AddControllers(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddSwaggerGenConfig(this IServiceCollection services)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

        }



    }

}
