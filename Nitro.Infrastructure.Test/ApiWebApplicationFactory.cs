using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nitro.Core.Interfaces.Cms;
using Nitro.Infrastructure.Data;
using Nitro.Kernel.Interfaces.Data;
using Nitro.Service.Cms;
using Xunit;

namespace Nitro.Infrastructure.Test
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        public IConfiguration Configuration { get; private set; }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("integrationsettings.json")
                    .Build();
                config.AddConfiguration(Configuration);
            });
            builder.ConfigureTestServices(services =>
            {
                services.AddTransient<IQueryRepository, QueryRepository>();
                services.AddTransient<ICommandRepository, CommandRepository>();
                services.AddTransient<IAuthorService, AuthorService>();
            });
        }
    }
}