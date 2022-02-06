using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nitro.FileStorage.Models;
using Nitro.FileStorage.Services;

namespace Nitro.Web.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FileStorageController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly FileStorageService _fileStorageService;
        private readonly ILogger<FileStorageController> _logger;

        public FileStorageController(ILogger<FileStorageController> logger, FileStorageService fileStorageService)
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        [Route("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> GetWeatherForecast()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
           .ToArray();
        }

        [HttpGet]
        [Route("NewFile")]
        public async Task NewFile()
        {
            await _fileStorageService.CreateAsync(new FileObject()
            {
                FileName = "filenameExample",
                FileType = "jpg",
                Length = 255445,
                UploadDateTime = DateTime.Now,
                UserId = "username"
            });
        }

        [HttpGet]
        [Route("UploadFile")]
        public async Task UploadFile()
        {
            var fileStream = new FileStream(@"d:\file.mkv", FileMode.Open, FileAccess.Read);
            try
            {

               var id= await _fileStorageService.Upload("file.mkv", fileStream);
            }
            finally
            {
                fileStream.Close();
            }
        }
    }
}