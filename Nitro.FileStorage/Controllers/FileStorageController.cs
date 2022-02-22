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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileStorageController> _logger;

        public FileStorageController(
            ILogger<FileStorageController> logger,
            IFileStorageService fileStorageService)
        {
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
            var fileStream = file.OpenReadStream();
            var fileValidation =
                await _fileStorageService.ValidateFileAsync(fileStream, filename, FileSizeEnum.Small,
                    cancellationToken);
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

            var contentType = file.ContentType;

            var objectId =
                await _fileStorageService.UploadFromStreamAsync(filename, contentType, fileStream, cancellationToken);
            var fileUploadResult = new FileUploadResultModel()
                {ObjectId = objectId.ToString(), FileName = filename, IsSuccessful = true};
            return Ok(fileUploadResult);

        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [Route(nameof(UploadMultipleFiles))]
        public async Task<IActionResult> UploadMultipleFiles(IFormFileCollection files,
            CancellationToken cancellationToken)
        {
            var filesUploadResult = new List<FileUploadResultModel>();

            foreach (var file in files)
            {
                var filename = file.FileName;
                var fileValidation = await _fileStorageService.ValidateFileAsync(file.OpenReadStream(), filename,
                    FileSizeEnum.Small, cancellationToken);
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

                var contentType = file.ContentType;
                var objectId = await _fileStorageService.UploadFromStreamAsync(filename, contentType,
                    file.OpenReadStream(),
                    cancellationToken);
                filesUploadResult.Add(new FileUploadResultModel()
                    {ObjectId = objectId.ToString(), FileName = filename, IsSuccessful = true});
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
                var section = await reader.ReadNextSectionAsync(cancellationToken);
                if (section == null) 
                    return BadRequest("No files data in the request.");

                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (!hasContentDispositionHeader || !contentDisposition.DispositionType.Equals("form-data") ||
                    string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    return BadRequest("No files data in the request.");


                var fileSection = section.AsFileSection();
                var contentType = section.ContentType;
                var fileName = Path.GetFileName(fileSection?.FileName);

                ////var fileValidation =
                ////    await _fileStorageService.ValidateFileAsync(section.Body, fileName, FileSizeEnum.Large,
                ////        cancellationToken);
                ////if (fileValidation != ValidationFileEnum.Ok)
                ////{
                ////    return Ok(new FileUploadResultModel()
                ////    {
                ////        ObjectId = null,
                ////        FileName = fileName,
                ////        IsSuccessful = false,
                ////        ErrorMessage = GetValidationResult(fileValidation)
                ////    });

                ////}

                var objectId = await _fileStorageService.UploadFromStreamAsync(fileName, contentType,
                    section.Body, cancellationToken);

                return Ok(new FileUploadResultModel()
                    {ObjectId = objectId.ToString(), FileName = fileName, IsSuccessful = true});

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

                        var fileValidation =
                            await _fileStorageService.ValidateFileAsync(section.Body, fileName, FileSizeEnum.Large,
                                cancellationToken);
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

                        var objectId = await _fileStorageService.UploadFromStreamAsync(fileName, contentType,
                            section.Body, cancellationToken);
                        filesUploadResult.Add(new FileUploadResultModel()
                            {ObjectId = objectId.ToString(), FileName = fileName, IsSuccessful = true});

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
            var parsedObjectId = new ObjectId(objectId);
            var result = await _fileStorageService.DownloadAsBytesAsync(parsedObjectId);
            if (result == null)
            {
                return BadRequest("file Not Found.");
            }

            var metadata = result.FileInfo.Metadata;
            var contentType = metadata.GetElement("ContentType").Value.ToString();
            var fileName = metadata.GetElement("UntrustedFileName").Value.ToString();
            return new FileContentResult(result.FileBytes, contentType ?? "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }

        [HttpGet]
        [Route(nameof(DownloadFileStream))]
        public async Task<IActionResult> DownloadFileStream(string objectId)
        {
            await using var stream = new MemoryStream();

            var parsedObjectId = new ObjectId(objectId);
            var result = await _fileStorageService.DownloadToStreamAsync(parsedObjectId, stream);

            if (result == null)
            {
                return BadRequest("file Not Found.");
            }

            var metadata = result.FileInfo.Metadata;
            var contentType = metadata.GetElement("ContentType").Value.ToString();
            var fileName = metadata.GetElement("UntrustedFileName").Value.ToString();

            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }

        private static string GetValidationResult(ValidationFileEnum validationFileEnum)
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

                case ValidationFileEnum.Ok:
                default:
                    return "";
            }
        }

    }
}