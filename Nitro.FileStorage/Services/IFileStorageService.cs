using MongoDB.Bson;
using Nitro.FileStorage.Models;
using Nitro.FileStorage.Settings;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {
        ValidationFileEnum ValidateFile(IFormFile file, string fileSize = "small");

        Task<ObjectId> UploadFromBytesAsync(string filename, string contentType, byte[] bytes);

        Task<ObjectId> UploadFromStreamAsync(string filename, string contentType, Stream stream);

        Task<FileDownloadByteModel> DownloadAsBytesAsync(ObjectId objectId);

        Task<FileDownloadStreamModel> DownloadToStreamAsync(ObjectId objectId, Stream destination);


    }
}