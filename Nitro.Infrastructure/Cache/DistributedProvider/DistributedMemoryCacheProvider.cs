using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Infrastructure.Cache.DistributedProvider
{
    public static class DistributedMemoryCacheProvider
    {
        public static void AddDistributedMemoryCacheProvider(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache(options =>
            {
                options.SizeLimit = 1024;
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(10);
                options.CompactionPercentage = 25; // 25% of cached data will removed if memory exeeded 
            });

        }
    }
}
