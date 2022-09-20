using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nitro.Core.Interfaces.Settings;
using Nitro.Kernel.Interfaces;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Nitro.Infrastructure.Setting
{

    public static class SettingStartup
    {
        public static void AddSettingConfig(this WebApplicationBuilder builder)
        {
            // Add Email Setting to the container.
            builder.Services.AddSingleton<ISmtpSetting>((serviceProvider) =>
                builder.Configuration.GetSection("SmtpSetting").Get<SmtpSetting>());

            // Add Sms Setting to the container.
            builder.Services.AddSingleton<ISmsSetting>((serviceProvider) =>
                builder.Configuration.GetSection("SmsSetting").Get<SmsSetting>());


        }
        
    }
}
