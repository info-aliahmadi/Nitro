using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;

namespace Nitro.Infrastructure.Data
{
    public static class CacheStartup
    {
        public static void AddCacheConfig(this IServiceCollection services)
        {
            builder.Host.ConfigureAppConfiguration((context, config) =>

            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true);
            }).UseSerilog();

        }
    
    }
}