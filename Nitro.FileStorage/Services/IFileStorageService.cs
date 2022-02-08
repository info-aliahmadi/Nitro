using MongoDB.Bson;
using Nitro.FileStorage.Settings;

namespace Nitro.FileStorage.Services
{
    public interface IFileStorageService
    {
        ValidationFileEnum ValidateFile(IFormFile file, string fileSize = "small");

        Task<ObjectId> UploadFromBytesAsync(string filename, string contentType, byte[] bytes);

        Task<ObjectId> UploadFromStreamAsync(string filename, string contentType, Stream stream);

        Task<byte[]> DownloadAsync(ObjectId objectId);

        Task<Stream> DownloadToStreamAsync(ObjectId objectId, Stream destination);


    }
}