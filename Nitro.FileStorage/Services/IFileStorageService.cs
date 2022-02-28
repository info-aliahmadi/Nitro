using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {
        ValidationFileEnum ValidateFileWhiteList(string fileName);

        ValidationFileEnum ValidateFileSignature(byte[] file, string fileExtension);

        ValidationFileEnum ValidateFileLength(long lengthOfFile,
            FileSizeEnum fileSize = FileSizeEnum.Small);

        ValidationFileEnum ValidateFile(byte[] file, string? fileName, long? lengthOfFile = null, FileSizeEnum fileSize = FileSizeEnum.Small, CancellationToken cancellationToken = default);

        string GetValidationMessage(ValidationFileEnum validationFileEnum);
        Task<GridFSFileInfo?> GetFileInfo(ObjectId objectId);

        Task<FileUploadResultModel> UploadFromBytesAsync(string? fileName, string? contentType, byte[] bytes,
            CancellationToken cancellationToken = default);

        Task<FileUploadResultModel> UploadSmallFileFromStreamAsync(string? fileName, string? contentType, Stream stream,
            CancellationToken cancellationToken = default);

        Task<FileUploadResultModel> UploadLargeFileFromStreamAsync(string? fileName, string? contentType, Stream stream,
            CancellationToken cancellationToken = default);

        Task<FileUploadResultModel> UploadFromStreamAsync(
            string? fileName,
            string newFileName,
            FileSizeEnum fileSize,
            Stream source,
            GridFSUploadOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<byte[]> DownloadAsBytesAsync(ObjectId objectId, CancellationToken cancellationToken = default);

        Task DownloadToStreamAsync(ObjectId objectId, Stream destination, CancellationToken cancellationToken = default);


    }
}