using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
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

                var id = await _fileStorageService.Upload("file.mkv", fileStream);
            }
            finally
            {
                fileStream.Close();
            }
        }

        [HttpGet]
        [Route(nameof(DownloadFileStream))]
        public async Task<FileStreamResult> DownloadFileStream(string objectId)
        {
            var fileStream = new FileStream(@"d:\file.mkv", FileMode.Open, FileAccess.Read);

            return new FileStreamResult(fileStream, "application/octet-stream")
            {
                FileDownloadName = "FileDownloadName.jpg"
            };

        }

        [HttpGet]
        [Route(nameof(DownloadFileStream))]
        public async Task<FileContentResult> DownloadFile(string objectId)
        {
            var fileBytes = System.IO.File.ReadAllBytes(@"d:\file.mkv");
            //(@"d:\file.mkv", FileMode.Open, FileAccess.Read);

            return new FileContentResult(fileBytes, "application/octet-stream")
            {
                FileDownloadName = "FileDownloadName.jpg"
            };

        }

        [HttpPost]

        [DisableFormValueModelBinding]
        [Route(nameof(UploadFileStream))]
        public async Task<IActionResult> UploadFileStream()
        {
            var request = HttpContext.Request;

            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a MediaType should be found in the Content-Type
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
            var section = await reader.ReadNextSectionAsync();

            // This sample try to get the first file from request and save it
            // Make changes according to your needs in actual use
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                    // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                    // In short, it is necessary to restrict and verify the upload
                    // Here, we just use the temporary folder and a random file name

                    // Get the temporary folder, and combine a random file name with it
                    var fileName = Path.GetRandomFileName();
                    var saveToPath = Path.Combine(Path.GetTempPath(), fileName);

                    
                        Stream stream = new();
                        await section.Body.CopyToAsync(stream);
                    

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return BadRequest("No files data in the request.");

        }
    }
}