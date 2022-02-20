using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using Nitro.FileStorage.Infrastructure;
using Nitro.FileStorage.Infrastructure.Settings;
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

        private readonly IUploadFileSetting _fileStorageSetting;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileStorageController> _logger;
        const int maxSmallFile = 0;

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
        [DisableFormValueModelBinding]
        //[GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            var filename = file.FileName;
            var fileValidation = _fileStorageService.ValidateFile(file.OpenReadStream(), filename);
            if (fileValidation != ValidationFileEnum.Ok)
            {
                return Ok(new FileUploadResultModel()
                {
                    ObjectId = null,
                    FileName = filename,
                    IsSuccessful = false,
                    ErrorMessage = GetValidationResult(fileValidation)
                });
            }
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var contentType = file.ContentType;

                var objectId = await _fileStorageService.UploadFromStreamAsync(filename, contentType, memoryStream, cancellationToken);
                var fileUploadResult = new FileUploadResultModel() { ObjectId = objectId.ToString(), FileName = filename, IsSuccessful = true };
                return Ok(fileUploadResult);
            }
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [Route(nameof(UploadMultipleFiles))]
        public async Task<IActionResult> UploadMultipleFiles(IFormFileCollection files, CancellationToken cancellationToken)
        {
            var filesUploadResult = new List<FileUploadResultModel>();

            foreach (var file in files)
            {
                var filename = file.FileName;
                var fileValidation = _fileStorageService.ValidateFile(file.OpenReadStream(), filename);
                if (fileValidation != ValidationFileEnum.Ok)
                {
                    filesUploadResult.Add(new FileUploadResultModel()
                    {
                        ObjectId = null,
                        FileName = filename,
                        IsSuccessful = false,
                        ErrorMessage = GetValidationResult(fileValidation)
                    });
                    continue;
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var contentType = file.ContentType;
                    var objectId = await _fileStorageService.UploadFromStreamAsync(filename, contentType, memoryStream, cancellationToken);
                    filesUploadResult.Add(new FileUploadResultModel() { ObjectId = objectId.ToString(), FileName = filename, IsSuccessful = true });
                }
            }
            return Ok(filesUploadResult);
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [Route(nameof(UploadLargeFile))]
        public async Task<IActionResult> UploadLargeFile(CancellationToken cancellationToken)
        {
            try
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
                if (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                       out var contentDisposition);

                    if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                        !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        var fileSection = section.AsFileSection();
                        var contentType = section.ContentType;
                        var fileName = Path.GetFileName(fileSection.FileName);

                        const int chunkSize = 64;
                        var instanceBuffer = new byte[chunkSize];
                        await fileSection.FileStream.ReadAsync(instanceBuffer);
                        
                        var fileValidation = _fileStorageService.ValidateFile(instanceBuffer, request.ContentLength ?? 0,
                            fileName, FileSizeEnum.Large);
                        if (fileValidation != ValidationFileEnum.Ok)
                        {
                            return Ok(new FileUploadResultModel()
                            {
                                ObjectId = null,
                                FileName = fileName,
                                IsSuccessful = false,
                                ErrorMessage = GetValidationResult(fileValidation)
                            });

                        }

                        var objectId = await _fileStorageService.UploadFromStreamAsync(fileName, contentType, section.Body, cancellationToken);

                        return Ok(new FileUploadResultModel() { ObjectId = objectId.ToString(), FileName = fileName ,IsSuccessful = true});
                    }
                }
                return BadRequest("No files data in the request.");

            }
            catch (Exception)
            {
                return BadRequest("No files data in the request.");
            }
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [Route(nameof(UploadMultipleLargeFiles))]
        public async Task<IActionResult> UploadMultipleLargeFiles(CancellationToken cancellationToken)
        {
            try
            {
                var filesUploadResult = new List<FileUploadResultModel>();
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
                var section = await reader.ReadNextSectionAsync(cancellationToken);

                // This sample try to get the first file from request and save it
                // Make changes according to your needs in actual use
                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                        out var contentDisposition);

                    if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                        !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        var fileSection = section.AsFileSection();
                        if (fileSection == null)
                        {
                            filesUploadResult.Add(new FileUploadResultModel()
                            {
                                ObjectId = null,
                                FileName = "Unknown",
                                IsSuccessful = false,
                                ErrorMessage = "Unknown content"
                            });

                            section = await reader.ReadNextSectionAsync(cancellationToken);
                            continue;
                        }
                        var contentType = section.ContentType ?? string.Empty;

                        var fileName = Path.GetFileName(fileSection.FileName);

                        const int chunkSize = 64;
                        var instanceBuffer = new byte[chunkSize];
                        _ = await fileSection.FileStream.ReadAsync(instanceBuffer, cancellationToken);
                        var contentLength = await _fileStorageService.GetLengthOfStream(section.Body);

                        var fileValidation = _fileStorageService.ValidateFile(instanceBuffer, contentLength, fileName, FileSizeEnum.Large);
                        if (fileValidation != ValidationFileEnum.Ok)
                        {
                            filesUploadResult.Add(new FileUploadResultModel()
                            {
                                ObjectId = null,
                                FileName = fileName,
                                IsSuccessful = false,
                                ErrorMessage = GetValidationResult(fileValidation)
                            });

                            section = await reader.ReadNextSectionAsync(cancellationToken);
                            continue;
                        }

                        var objectId = await _fileStorageService.UploadFromStreamAsync(fileName, contentType, section.Body, cancellationToken);
                        filesUploadResult.Add(new FileUploadResultModel() { ObjectId = objectId.ToString(), FileName = fileName, IsSuccessful = true });

                    }

                    section = await reader.ReadNextSectionAsync(cancellationToken);
                }
                return Ok(filesUploadResult);
            }
            catch (Exception)
            {
                // If the code runs to this location, it means that no files have been saved
                return BadRequest("No files data in the request.");
            }
        }


        [HttpGet]
        [Route(nameof(DownloadFile))]
        public async Task<IActionResult> DownloadFile(string objectId)
        {
            ObjectId parsedObjectId = new ObjectId(objectId);
            var result = await _fileStorageService.DownloadAsBytesAsync(parsedObjectId);
            if (result.FileInfo == null)
            {
                return BadRequest("file Not Found.");
            }
            var metadata = result.FileInfo.Metadata;
            var contentType = metadata.GetElement("ContentType").Value.ToString();
            var fileName = metadata.GetElement("UntrustedFileName").Value.ToString();
            return new FileContentResult(result.FileBytes, contentType)
            {
                FileDownloadName = fileName
            };
        }

        [HttpGet]
        [Route(nameof(DownloadFileStream))]
        public async Task<IActionResult> DownloadFileStream(string objectId)
        {
            var memoryStream = new MemoryStream();
            ObjectId parsedObjectId = new ObjectId(objectId);
            var result = await _fileStorageService.DownloadToStreamAsync(parsedObjectId, memoryStream);

            if (result.FileInfo == null)
            {
                return BadRequest("file Not Found.");
            }
            var metadata = result.FileInfo.Metadata;
            var contentType = metadata.GetElement("ContentType").Value.ToString();
            var fileName = metadata.GetElement("UntrustedFileName").Value.ToString();

            return new FileStreamResult(memoryStream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };

        }

        private string GetValidationResult(ValidationFileEnum validationFileEnum)
        {
            switch (validationFileEnum)
            {
                case ValidationFileEnum.FileNotFound:
                    // If the code runs to this location, it means that no files have been saved
                    return "No files data in the request.";

                case ValidationFileEnum.FileIsTooLarge:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file is too large.";

                case ValidationFileEnum.FileIsTooSmall:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file is too small.";

                case ValidationFileEnum.FileNotSupported:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file is not supported.";

                case ValidationFileEnum.InvalidSignature:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file extension is not trusted.";

                default:
                    return "";
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