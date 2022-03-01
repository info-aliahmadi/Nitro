using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {

        Task<GridFSFileInfo?> GetFileInfoById(ObjectId objectId);
        Task<GridFSFileInfo?> GetFileInfoByMd5HashCode(string md5HashCode);
        string GetMd5HashCode(byte[] bytes);

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

        Task DownloadToStreamAsync(ObjectId objectId, Stream destination,
            CancellationToken cancellationToken = default);


    }
}