using Nitro.Infrastructure.Configuration;
using Nitro.Infrastructure.Data;
using Serilog;
using System.Reflection;


	SerilogStartup.ConfigureLogging();

	try
	{
		var builder = WebApplication.CreateBuilder(args);


		builder.Services.ConfigureApplicationServices(builder);


		var app = builder.Build();

		// Configure the HTTP request pipeline.
		app.ConfigureRequestPipeline();


		app.Run();
	}
	catch (System.Exception ex)
	{
		Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);
		throw;
	}