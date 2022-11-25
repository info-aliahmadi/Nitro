using Nitro.FileStorage.Infrastructure;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Infrastructure.SignatureVerify;
using Nitro.FileStorage.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:7295",
                "https://localhost:3000", "http://localhost:3000").AllowAnyOrigin().AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddSingleton<IFileStorageDatabaseSetting>((serviceProvider) =>
    builder.Configuration.GetSection("FileStorageDatabase").Get<FileStorageDatabaseSetting>());

// Add services to the container.
builder.Services.AddSingleton<IUploadFileSetting>((serviceProvider) =>
    builder.Configuration.GetSection("UploadFileSetting").Get<UploadFileSetting>());

builder.Services.AddSingleton<IFileTypeVerifier, FileTypeVerifier>();
builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddHostedService<AutoDeleteService>(); // Auto delete temp file

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseHttpsRedirection();


app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();
