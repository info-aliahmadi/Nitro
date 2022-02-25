using MongoDB.Bson;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {
        Task<ValidationFileEnum> ValidateFileAsync(byte[] file, string? fileName, long? lengthOfFile = null, FileSizeEnum fileSize = FileSizeEnum.Small, CancellationToken cancellationToken = default);

        string GetValidationMessage(ValidationFileEnum validationFileEnum);

        Task<FileUploadResultModel> UploadFromBytesAsync(string? filename, string? contentType, byte[] bytes, CancellationToken cancellationToken = default);

        Task<FileUploadResultModel> UploadFromStreamAsync(string? filename, string? contentType, Stream stream, CancellationToken cancellationToken = default);

        Task<FileDownloadModel?> DownloadAsBytesAsync(ObjectId objectId, CancellationToken cancellationToken = default);

        Task<FileDownloadModel?> DownloadToStreamAsync(ObjectId objectId, Stream destination, CancellationToken cancellationToken = default);


    }
}