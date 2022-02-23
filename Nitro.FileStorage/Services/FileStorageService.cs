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

           // mongoDatabase.AggregateToCollection(PipelineDefinition<null,>.Create(),new AggregateOptions(){AllowDiskUse = true});

           ImagesBucket = new GridFSBucket(mongoDatabase
               , new GridFSBucketOptions
               {
                   BucketName = "Nitro",
               //    ChunkSizeBytes = 1048576, // 1MB
               //    WriteConcern = WriteConcern.WMajority,
               //    ReadPreference = ReadPreference.Secondary
               }
           );

        }

        public async Task<long> GetLengthOfStream(Stream file, CancellationToken cancellationToken = default)
        {
            long length = 0;
            byte[] buffer = new byte[2048]; // read in chunks of 2KB
            while (await file.ReadAsync(buffer, 0, buffer.Length, cancellationToken) > 0)
            {
                length += buffer.Length;
            }

            return length;
        }

        public async Task<long> GetLengthOfStreamWithLimitation(Stream file, long lengthLimit,
            CancellationToken cancellationToken = default)
        {
            long length = 0;
            byte[] buffer = new byte[2048]; // read in chunks of 2KB
            while (await file.ReadAsync(buffer, 0, buffer.Length, cancellationToken) > 0 && length < (lengthLimit+2))
            {
                length += buffer.Length;
            }

            return length;
        }

        public async Task<ValidationFileEnum> ValidateFileAsync(Stream file, string? fileName,
            FileSizeEnum fileSize = FileSizeEnum.Small, CancellationToken cancellationToken = default)
        {
            long length = 0;
            if (string.IsNullOrEmpty(fileName))
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotFound;
            }


            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            var whiteListExtensions = _fileStorageSetting.WhiteListExtensions.Split(",");
            if (!whiteListExtensions.Contains(fileExtension))
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotSupported;
            }

            if (fileSize == FileSizeEnum.Small)
            {

                length = file.Length;
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
                length = await GetLengthOfStreamWithLimitation(file, _fileStorageSetting.MaxSizeLimitLargeFile,
                    cancellationToken);
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

            var signatureValidationExtensions = _fileStorageSetting.SignatureValidationExtensions.Split(",");

            if (signatureValidationExtensions.Contains(fileExtension))
            {
                // we will see how we can protect the integrity of our file uploads by
                // verifying the files are what the user says they are
                var verifySignature = await _fileTypeVerifier.VerifyAsync(file, fileExtension);
                if (!verifySignature.IsVerified)
                {
                    return ValidationFileEnum.InvalidSignature;
                }
            }

            return ValidationFileEnum.Ok;
        }


        public async Task<GridFSFileInfo?> GetFileInfo(ObjectId objectId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, objectId));

            var cursor = await ImagesBucket.FindAsync(new BsonDocument {{"_id", objectId}});

            var result = (await cursor.ToListAsync()).FirstOrDefault();

            return result ?? null;
        }

        public async Task<ObjectId> UploadFromBytesAsync(string? filename, string? contentType, byte[] bytes,
            CancellationToken cancellationToken = default)
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

        public async Task<ObjectId> UploadFromStreamAsync(string? filename, string? contentType, Stream stream,
            CancellationToken cancellationToken = default)
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

        public async Task<FileDownloadByteModel?> DownloadAsBytesAsync(ObjectId objectId,
            CancellationToken cancellationToken = default)
        {
            var fileInfo = await GetFileInfo(objectId);
            if (fileInfo == null)
            {
                return null;
            }
            var options = new GridFSDownloadOptions
            {
                Seekable = true,
                
            };
            var bytes = await ImagesBucket.DownloadAsBytesAsync(fileInfo.Id, options, cancellationToken);

            var result = new FileDownloadByteModel()
            {
                ObjectId = objectId,
                FileInfo = fileInfo,
                FileBytes = bytes
            };

            return result;
        }

        public async Task<FileDownloadStreamModel?> DownloadToStreamAsync(ObjectId objectId, Stream destination,
            CancellationToken cancellationToken = default)
        {
            var fileInfo = await GetFileInfo(objectId);
            if (fileInfo == null)
            {
                return null;
            }

            var result = new FileDownloadStreamModel()
            {
                ObjectId = objectId,
                FileInfo = fileInfo
            };
            await ImagesBucket.DownloadToStreamAsync(objectId, destination, null, cancellationToken);
            return result;

        }
    }
}