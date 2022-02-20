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
        public GridFSBucket ImagesBucket { get; set; }

        public FileStorageService(
            IFileStorageDatabaseSetting fileStorageDatabaseSetting,
            IUploadFileSetting fileStorageSetting,
            IFileTypeVerifier fileTypeVerifier)
        {
            _fileStorageSetting = fileStorageSetting;
            _fileTypeVerifier = fileTypeVerifier;

            var mongoClient = new MongoClient(
                fileStorageDatabaseSetting.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                fileStorageDatabaseSetting.DatabaseName);

            ImagesBucket = new GridFSBucket(mongoDatabase);
        }

        public async Task<long> GetLengthOfStream(Stream file)
        {
            long length = 0;
            byte[] buffer = new byte[2048]; // read in chunks of 2KB
            while (await file.ReadAsync(buffer, 0, buffer.Length) > 0)
            {
                length += buffer.Length;
            }

            return length;
        }

        public ValidationFileEnum ValidateFile(byte[] file, long length, string? fileName, FileSizeEnum fileSize = FileSizeEnum.Small)
        {
            if (file.Length == 0 || fileName == null)
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotFound;

            }
            if (fileSize == FileSizeEnum.Small)
            {
                if (length > _fileStorageSetting.MaxSizeLimitSmallFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooLarge;
                }
                if (length < _fileStorageSetting.MinSizeLimitSmallFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooSmall;
                }

            }
            else if (fileSize == FileSizeEnum.Large)
            {
                if (length > _fileStorageSetting.MaxSizeLimitLargeFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooLarge;
                }
                if (length < _fileStorageSetting.MinSizeLimitLargeFile)
                {
                    // If the code runs to this location, it means that no files have been saved
                    return ValidationFileEnum.FileIsTooSmall;
                }
            }

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

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
                var verifySignature = _fileTypeVerifier.Verify(file, fileExtension);
                if (!verifySignature.IsVerified)
                {
                    return ValidationFileEnum.InvalidSignature;
                }
            }

            return ValidationFileEnum.Ok;
        }

        public ValidationFileEnum ValidateFile(Stream? file, string? fileName, FileSizeEnum fileSize = FileSizeEnum.Small)
        {
            if (file == null || fileName == null)
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotFound;

            }
            if (fileSize == FileSizeEnum.Small)
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
            else if (fileSize == FileSizeEnum.Large)
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

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

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
                //var verifySignature = _fileTypeVerifier.Verify(file, fileExtension);
                //if (!verifySignature.IsVerified)
                //{
                //    return ValidationFileEnum.InvalidSignature;
                //}
            }


            return ValidationFileEnum.Ok;
        }

        public async Task<GridFSFileInfo> GetFileInfo(ObjectId objectId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, objectId));

            var cursor = await ImagesBucket.FindAsync(new BsonDocument { { "_id", objectId } });

            var result = (await cursor.ToListAsync()).FirstOrDefault();

            if (result == null)
            {
                return new GridFSFileInfo(null);
            }

            return result;
        }

        public async Task<ObjectId> UploadFromBytesAsync(string? filename, string? contentType, byte[] bytes, CancellationToken cancellationToken = default)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType", contentType},
                    {"UntrustedFileName", filename}
                }
            };
            // Don't trust any file name, file extension, and file data from the request unless you trust them completely
            // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
            // In short, it is necessary to restrict and verify the upload
            // Here, we just use the temporary folder and a random file name
            var newFileName = Path.GetRandomFileName();
            var id = await ImagesBucket.UploadFromBytesAsync(newFileName, bytes, options, cancellationToken);
            return id;
        }
        public async Task<ObjectId> UploadFromStreamAsync(string? filename, string? contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType", contentType},
                    {"UntrustedFileName", filename}
                }
            };
            // Don't trust any file name, file extension, and file data from the request unless you trust them completely
            // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
            // In short, it is necessary to restrict and verify the upload
            // Here, we just use the temporary folder and a random file name
            var newFileName = Path.GetRandomFileName();

            //NetworkStream Stream = stream;


            var id = await ImagesBucket.UploadFromStreamAsync(newFileName, stream, options, cancellationToken);
            return id;
        }

        public async Task<FileDownloadByteModel> DownloadAsBytesAsync(ObjectId objectId, CancellationToken cancellationToken = default)
        {
            var result = new FileDownloadByteModel() { 
                ObjectId = objectId,
                FileInfo = await GetFileInfo(objectId),
                FileBytes = await ImagesBucket.DownloadAsBytesAsync(objectId,null, cancellationToken)

            };

            return result;
        }
        public async Task<FileDownloadStreamModel>  DownloadToStreamAsync(ObjectId objectId, Stream destination, CancellationToken cancellationToken = default)
        {
            var result = new FileDownloadStreamModel() { ObjectId = objectId };

            result.FileInfo = await GetFileInfo(objectId);

            await ImagesBucket.DownloadToStreamAsync(objectId, destination,null,cancellationToken);

            return result;
        }

    }
}
