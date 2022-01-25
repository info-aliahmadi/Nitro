using Nitro.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureApplicationServices(builder);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureRequestPipeline();


app.Run();
