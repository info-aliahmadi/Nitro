using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nitro.FileStorage.Infrastructure.Settings;
using Nitro.FileStorage.Infrastructure.SignatureVerify;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileTypeVerifier _fileTypeVerifier;
        private readonly IUploadFileSetting _fileStorageSetting;
        public GridFSBucket _imagesBucket { get; set; }

        public FileStorageService(
            IOptions<IFileStorageDatabaseSetting> fileStorageDatabaseSetting,
            IUploadFileSetting fileStorageSetting,
            IFileTypeVerifier fileTypeVerifier)
        {
            _fileStorageSetting = fileStorageSetting;
            _fileTypeVerifier = fileTypeVerifier;

            var mongoClient = new MongoClient(
                fileStorageDatabaseSetting.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                fileStorageDatabaseSetting.Value.DatabaseName);

            _imagesBucket = new GridFSBucket(mongoDatabase);
        }


        public ValidationFileEnum ValidateFile(IFormFile file, string fileSize = "small")
        {
            if (file == null)
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotFound;

            }
            if (fileSize == "small")
            {
                var fileLength = file.Length;
                if (fileLength > _fileStorageSetting.MaxSizeLimitSmallFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooLarge;
                }
                if (fileLength < _fileStorageSetting.MinSizeLimitSmallFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooSmall;
                }

            }
            else if (fileSize == "large")
            {
                var fileLength = file.Length;
                if (fileLength > _fileStorageSetting.MaxSizeLimitLargeFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooLarge;
                }
                if (fileLength < _fileStorageSetting.MinSizeLimitLargeFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooSmall;
                }
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var whiteListExtensions = _fileStorageSetting.WhiteListExtensions.Split(",");
            if (!whiteListExtensions.Contains(fileExtension))
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotSupported;
            }

            var signatureValidationExtensions = _fileStorageSetting.SignatureValidationExtensions.Split(",");

            if (signatureValidationExtensions.Contains(fileExtension))
            {
                // we will see how we can protect the integrity of our file uploads by
                // verifying the files are what the user says they are
                var verifySignature = _fileTypeVerifier.Verify(file.OpenReadStream(), fileExtension);
                if (!verifySignature.IsVerified)
                {
                    return ValidationFileEnum.InvalidSignature;
                }
            }


            return ValidationFileEnum.Ok;
        }

        public async Task<GridFSFileInfo> GetFileInfo(ObjectId objectId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, objectId));

            var cursor = await _imagesBucket.FindAsync(filter);

            var result = (await cursor.ToListAsync()).FirstOrDefault();

            if (result == null)
            {
                return new GridFSFileInfo(null);
            }

            return result;
        }

        public async Task<ObjectId> UploadFromBytesAsync(string filename, string contentType, byte[] bytes)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType",contentType},
                    {"UntrustedFileName",filename}
                }
            };
            var id = await _imagesBucket.UploadFromBytesAsync(filename, bytes, options);
            return id;
        }
        public async Task<ObjectId> UploadFromStreamAsync(string filename, string contentType, Stream stream)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType",contentType},
                    {"UntrustedFileName",filename}
                }
            };
            var id = await _imagesBucket.UploadFromStreamAsync(filename, stream, options);
            return id;
        }

        public async Task<FileDownloadByteModel> DownloadAsBytesAsync(ObjectId objectId)
        {
            var result = new FileDownloadByteModel() { ObjectId = objectId };

            result.FileInfo = await GetFileInfo(objectId);

            result.FileBytes = await _imagesBucket.DownloadAsBytesAsync(objectId);

            return result;
        }
        public async Task<FileDownloadStreamModel>  DownloadToStreamAsync(ObjectId objectId, Stream destination)
        {
            var result = new FileDownloadStreamModel() { ObjectId = objectId };

            result.FileInfo = await GetFileInfo(objectId);

            await _imagesBucket.DownloadToStreamAsync(objectId, destination);

            return result;
        }

    }
}
