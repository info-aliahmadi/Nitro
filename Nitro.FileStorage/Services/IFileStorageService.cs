using MongoDB.Bson;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {
        ValidationFileEnum ValidateFile(Stream file, string fileName, FileSizeEnum fileSize = FileSizeEnum.Small);

        Task<ObjectId> UploadFromBytesAsync(string filename, string contentType, byte[] bytes);

        Task<ObjectId> UploadFromStreamAsync(string filename, string contentType, Stream stream);

        Task<FileDownloadByteModel> DownloadAsBytesAsync(ObjectId objectId);

        Task<FileDownloadStreamModel> DownloadToStreamAsync(ObjectId objectId, Stream destination);


    }
}