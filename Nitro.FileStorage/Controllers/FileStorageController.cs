using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using Nitro.FileStorage.Models;
using Nitro.FileStorage.Services;
using Nitro.FileStorage.Settings;

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

        private readonly IUploadFileSetting _fileStorageSetting;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileStorageController> _logger;

        public FileStorageController(
            IUploadFileSetting fileStorageSetting,
            ILogger<FileStorageController> logger, 
            IFileStorageService fileStorageService)
        {
            _fileStorageSetting = fileStorageSetting;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var fileValidation = _fileStorageService.ValidateFile(file);
            if (fileValidation != ValidationFileEnum.Ok)
            {
                return GetValidationResult(fileValidation);
            }
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var filename = Path.GetFileName(file.FileName);
                var contentType = file.ContentType;

                var objectId = _fileStorageService.UploadFromStreamAsync(filename, contentType, memoryStream);

                return Ok(objectId.ToString());
            }

        }
        [HttpPost]
        [Route(nameof(UploadMultipleFiles))]
        public async Task<IActionResult> UploadMultipleFiles(IFormFile[] files)
        {

            return Ok("");
        }

        [HttpPost]
        [Route(nameof(UploadLargeFile))]
        public async Task<IActionResult> UploadLargeFile()
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

                    using (var memoryStream = new MemoryStream())
                    {
                        await section.Body.CopyToAsync(memoryStream);
                    }

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return BadRequest("No files data in the request.");

        }

        [HttpGet]
        [Route(nameof(DownloadFileStream))]
        public async Task<FileStreamResult> DownloadFileStream(string objectId)
        {
            var memoryStream = new MemoryStream();
            ObjectId parsedObjectId = new ObjectId(objectId);
            var fileStream =await _fileStorageService.DownloadToStreamAsync(parsedObjectId, memoryStream);

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

        private IActionResult GetValidationResult(ValidationFileEnum validationFileEnum)
        {
            switch (validationFileEnum)
            {
                case ValidationFileEnum.FileNotFound:
                    // If the code runs to this location, it means that no files have been saved
                    return BadRequest("No files data in the request.");

                case ValidationFileEnum.FileIsTooLarge:
                    // If the code runs to this location, it means that no files have been saved
                    return BadRequest("The file is too large.");

                case ValidationFileEnum.FileIsTooSmall:
                    // If the code runs to this location, it means that no files have been saved
                    return BadRequest("The file is too small.");

                case ValidationFileEnum.FileNotSupported:
                    // If the code runs to this location, it means that no files have been saved
                    return BadRequest("The file is not suppot.");

                case ValidationFileEnum.InvalidSignature:
                    // If the code runs to this location, it means that no files have been saved
                    return BadRequest("The file extension is not trusted.");

                default:
                    return Ok();
            }
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

        //[HttpGet]
        //[Route("NewFile")]
        //public async Task NewFile()
        //{
        //    await _fileStorageService.CreateAsync(new FileObject()
        //    {
        //        FileName = "filenameExample",
        //        FileType = "jpg",
        //        Length = 255445,
        //        UploadDateTime = DateTime.Now,
        //        UserId = "username"
        //    });
        //}

        //[HttpGet]
        //[Route("UploadFile")]
        //public async Task UploadFile()
        //{
        //    var fileStream = new FileStream(@"d:\file.mkv", FileMode.Open, FileAccess.Read);
        //    try
        //    {

        //        var id = await _fileStorageService.Upload("file.mkv", fileStream);
        //    }
        //    finally
        //    {
        //        fileStream.Close();
        //    }
        //}


    }
}