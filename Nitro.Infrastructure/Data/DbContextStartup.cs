﻿using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Infrastructure.Data
{
    public static class ControllerStartup
    {
        public static void AddDbContextConfig(this IServiceCollection services,
            IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            // the default pool size in 1024 
            //Make sure that the maxPoolSize corresponds to your usage scenario;
            //if it is too low, DbContext instances will be constantly created and disposed,degrading performance.
            //Setting it too high may needlessly consume memory as
            //unused DbContext instances are maintained in the pool.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)); // the default pool size in 1024 

            services.AddDatabaseDeveloperPageExceptionFilter();

        }
    }
}
