using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using Nitro.FileStorage.Infrastructure.ActionFilters;
using Nitro.FileStorage.Models;
using Nitro.FileStorage.Services;

namespace Nitro.FileStorage.Controllers
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableFormValueModelBinding]
        //[GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            var filename = file.FileName;
            var contentType = file.ContentType;
            var stream = file.OpenReadStream();
            var result =
                await _fileStorageService.UploadSmallFileFromStreamAsync(filename, contentType, stream,
                    cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

                var contentType = file.ContentType;
                var result = await _fileStorageService.UploadSmallFileFromStreamAsync(filename, contentType,
                    file.OpenReadStream(),
                    cancellationToken);
                filesUploadResult.Add(result);
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
                if (fileSection == null)
                {
                    var unknown = new FileUploadResultModel()
                    {
                        ObjectId = null,
                        FileName = "Unknown",
                        IsSuccessful = false,
                        ErrorMessage = "Unknown content"
                    };
                    return Ok(unknown);

                }

                var contentType = section.ContentType;
                var fileName = Path.GetFileName(fileSection?.FileName);

                var result = await _fileStorageService.UploadLargeFileFromStreamAsync(fileName, contentType,
                    section.Body, cancellationToken);

                return Ok(result);

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

                        var result = await _fileStorageService.UploadLargeFileFromStreamAsync(fileName, contentType,
                            section.Body, cancellationToken);
                        filesUploadResult.Add(result);

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
        public async Task<IActionResult> DownloadFile(string objectId, CancellationToken cancellationToken)
        {
            var parsedObjectId = new ObjectId(objectId.Trim());

            var fileInfo = await _fileStorageService.GetFileInfoById(parsedObjectId);
            if (fileInfo == null)
            {
                return BadRequest("file Not Found.");
            }

            var fileBytes = await _fileStorageService.DownloadAsBytesAsync(parsedObjectId, cancellationToken);
           

            string? contentType = null;
            string? fileName = null;
            var metadata = fileInfo.Metadata;
            if (metadata != null)
            {
                contentType = metadata.GetElement("ContentType").Value.ToString();
                fileName = metadata.GetElement("UntrustedFileName").Value.ToString();
            }

            return new FileContentResult(fileBytes, contentType ?? "application/octet-stream")
            {
                FileDownloadName = fileName,
                EnableRangeProcessing = true // enable resume download ability
            };
        }

        [HttpGet]
        [Route(nameof(DownloadFileStream))]
        public async Task<IActionResult> DownloadFileStream(string objectId, CancellationToken cancellationToken)
        {
            var parsedObjectId = new ObjectId(objectId.Trim());

            var fileInfo = await _fileStorageService.GetFileInfoById(parsedObjectId);
            if (fileInfo == null)
            {
                return BadRequest("file Not Found.");
            }

            var tempFilePath = Directory.GetCurrentDirectory() + "\\Temp\\" + fileInfo.Filename;
            //Stream destination = new MemoryStream();  // using MemoryStream cause using a lot of memory
            Stream destination = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite);
            await _fileStorageService.DownloadToStreamAsync(parsedObjectId, destination, cancellationToken);

            destination.Seek(0, SeekOrigin.Begin);
            
            var metadata = fileInfo.Metadata;
            var fileName = metadata.GetElement("UntrustedFileName").Value.ToString();

            return new FileStreamResult(destination, "application/octet-stream")
            {
                FileDownloadName = fileName,
                EnableRangeProcessing = true // enable resume download ability
            };
        }

    }
}