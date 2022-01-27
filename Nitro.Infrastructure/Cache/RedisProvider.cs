using EasyCaching.Core.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Nitro.Infrastructure.Cache
{
    /// <summary>
    /// Distributed Cache Provider
    /// </summary>
    public static class RedisProvider
    {

        /// <summary>
        /// Distributed Cache Provider
        /// </summary>
        /// <param name="services"></param>
        public static void AddRedisCacheProvider(this IServiceCollection services,
            IConfiguration configuration, string providerName)
        {
            //configuration
            services.AddEasyCaching(options =>
            {
                //use redis cache that named redis1
                options.UseRedis(configuration, providerName);
            });
        }
    }
}
