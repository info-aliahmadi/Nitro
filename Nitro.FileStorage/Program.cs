using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Infrastructure.SignatureVerify;
using Nitro.FileStorage.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.Configure<IFileStorageDatabaseSetting>(
    builder.Configuration.GetSection("FileStorageDatabase"));

// Add services to the container.
builder.Services.Configure<IUploadFileSetting>(
    builder.Configuration.GetSection("UploadFileSetting"));

builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<IFileTypeVerifier, FileTypeVerifier>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();


app.MapControllers();

app.Run();
