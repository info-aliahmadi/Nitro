using MongoDB.Bson;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {
        Task<long> GetLengthOfStream(Stream file, CancellationToken cancellationToken = default);
        Task<long> GetLengthOfStreamWithLimitation(Stream file, long lengthLimit,
            CancellationToken cancellationToken = default);
        Task<ValidationFileEnum> ValidateFileAsync(Stream file, string? fileName, FileSizeEnum fileSize = FileSizeEnum.Small, CancellationToken cancellationToken = default);

        Task<ObjectId> UploadFromBytesAsync(string? filename, string? contentType, byte[] bytes, CancellationToken cancellationToken = default);

        Task<ObjectId> UploadFromStreamAsync(string? filename, string? contentType, Stream stream, CancellationToken cancellationToken = default);

        Task<FileDownloadByteModel?> DownloadAsBytesAsync(ObjectId objectId, CancellationToken cancellationToken = default);

        Task<FileDownloadStreamModel?> DownloadToStreamAsync(ObjectId objectId, Stream destination, CancellationToken cancellationToken = default);


    }
}