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
builder.Services.AddSingleton<IFileStorageDatabaseSetting>((serviceProvider) =>
{
    return builder.Configuration.GetSection("FileStorageDatabase").Get<FileStorageDatabaseSetting>();
});

// Add services to the container.
builder.Services.AddSingleton<IUploadFileSetting>((serviceProvider) =>
{
    return builder.Configuration.GetSection("UploadFileSetting").Get<UploadFileSetting>();
});

builder.Services.AddSingleton<IFileTypeVerifier, FileTypeVerifier>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

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
