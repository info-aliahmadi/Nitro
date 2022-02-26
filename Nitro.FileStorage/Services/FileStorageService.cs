using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
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
        public IMongoDatabase MongoDatabase { get; set; }

        public FileStorageService(
            IFileStorageDatabaseSetting fileStorageDatabaseSetting,
            IUploadFileSetting fileStorageSetting,
            IFileTypeVerifier fileTypeVerifier)
        {
            _fileStorageSetting = fileStorageSetting;
            _fileTypeVerifier = fileTypeVerifier;

            var mongoClient = new MongoClient(
                fileStorageDatabaseSetting.ConnectionString);

            MongoDatabase = mongoClient.GetDatabase(
                fileStorageDatabaseSetting.DatabaseName);
            // mongoDatabase.AggregateToCollection(PipelineDefinition<null,>.Create(),new AggregateOptions(){AllowDiskUse = true});

            ImagesBucket = new GridFSBucket(MongoDatabase
                , new GridFSBucketOptions
                {
                    BucketName = "Nitro",
                    //    ChunkSizeBytes = 1048576, // 1MB
                    //    WriteConcern = WriteConcern.WMajority,
                    //    ReadPreference = ReadPreference.Secondary
                }
            );
           

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ValidationFileEnum ValidateFileWhiteList(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            var whiteListExtensions = _fileStorageSetting.WhiteListExtensions.Split(",");
            if (!whiteListExtensions.Contains(fileExtension))
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotSupported;
            }
            return ValidationFileEnum.Ok;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public ValidationFileEnum ValidateFileSignature(byte[] file, string fileExtension)
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lengthOfFile"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public ValidationFileEnum ValidateFileMaxLength(long lengthOfFile,
            FileSizeEnum fileSize = FileSizeEnum.Small)
        {
            return fileSize switch
            {
                FileSizeEnum.Small when lengthOfFile > _fileStorageSetting.MaxSizeLimitSmallFile => ValidationFileEnum
                    .FileIsTooLarge,
                FileSizeEnum.Large when lengthOfFile > _fileStorageSetting.MaxSizeLimitLargeFile => ValidationFileEnum
                    .FileIsTooLarge,
                _ => ValidationFileEnum.Ok
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lengthOfFile"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public ValidationFileEnum ValidateFileMinLength(long lengthOfFile,
            FileSizeEnum fileSize = FileSizeEnum.Small)
        {
            return fileSize switch
            {

                FileSizeEnum.Small when lengthOfFile < _fileStorageSetting.MinSizeLimitSmallFile => ValidationFileEnum
                    .FileIsTooSmall,

                FileSizeEnum.Large when lengthOfFile < _fileStorageSetting.MinSizeLimitLargeFile => ValidationFileEnum
                    .FileIsTooSmall,
                _ => ValidationFileEnum.Ok
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lengthOfFile"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public ValidationFileEnum ValidateFileLength(long lengthOfFile,
            FileSizeEnum fileSize = FileSizeEnum.Small)
        {
            var result = ValidateFileMaxLength(lengthOfFile, fileSize);
            return result == ValidationFileEnum.Ok ? result : ValidateFileMinLength(lengthOfFile, fileSize);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="lengthOfFile"></param>
        /// <param name="fileSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValidationFileEnum ValidateFile(byte[] file, string? fileName,
            long? lengthOfFile = null,
            FileSizeEnum fileSize = FileSizeEnum.Small, CancellationToken cancellationToken = default)
        {
            var length = lengthOfFile ?? file.Length;
            if (string.IsNullOrEmpty(fileName))
            {
                // If the code runs to this location, it means that no files have been saved
                return ValidationFileEnum.FileNotFound;
            }

            var validateFileExtenstionResult = ValidateFileWhiteList(fileName);
            if (validateFileExtenstionResult != ValidationFileEnum.Ok)
            {
                return validateFileExtenstionResult;
            }


            var validateFileLengthResult = ValidateFileLength(length, fileSize);
            if (validateFileLengthResult != ValidationFileEnum.Ok)
            {
                return validateFileLengthResult;
            }

            var fileExtension = Path.GetExtension(fileName);
            var validateFileSignatureResult = ValidateFileSignature(file, fileExtension);
            if (validateFileSignatureResult != ValidationFileEnum.Ok)
            {
                return validateFileLengthResult;
            }

            return ValidationFileEnum.Ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationFileEnum"></param>
        /// <returns></returns>
        public string GetValidationMessage(ValidationFileEnum validationFileEnum)
        {
            switch (validationFileEnum)
            {
                case ValidationFileEnum.FileNotFound:
                    // If the code runs to this location, it means that no files have been saved
                    return "No files data in the request.";

                case ValidationFileEnum.FileIsTooLarge:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file is too large.";

                case ValidationFileEnum.FileIsTooSmall:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file is too small.";

                case ValidationFileEnum.FileNotSupported:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file is not supported.";

                case ValidationFileEnum.InvalidSignature:
                    // If the code runs to this location, it means that no files have been saved
                    return "The file extension is not trusted.";

                case ValidationFileEnum.Ok:
                default:
                    return "";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public async Task<GridFSFileInfo?> GetFileInfo(ObjectId objectId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, objectId));

            var cursor = await ImagesBucket.FindAsync(new BsonDocument {{"_id", objectId}});

            var result = (await cursor.ToListAsync()).FirstOrDefault();

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileUploadResultModel> UploadFromBytesAsync(string? fileName, string? contentType, byte[] bytes,
            CancellationToken cancellationToken = default)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType", contentType},
                    {"UntrustedFileName", fileName}
                }
            };
            // Don't trust any file name, file extension, and file data from the request unless you trust them completely
            // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
            // In short, it is necessary to restrict and verify the upload
            // Here, we just use the temporary folder and a random file name
            var newFileName = Path.GetRandomFileName();
            try
            {
               var id= await ImagesBucket.UploadFromBytesAsync(newFileName, bytes, options, cancellationToken);
                return new FileUploadResultModel()
                {
                    ObjectId = id.ToString(),
                    FileName = fileName
                };
            }
            catch (Exception e)
            {
                return new FileUploadResultModel()
                {
                    IsSuccessful = false,
                    ErrorMessage = e.Message + " " + e.InnerException
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileUploadResultModel> UploadSmallFileFromStreamAsync(string? fileName, string? contentType, Stream stream,
            CancellationToken cancellationToken = default)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType", contentType},
                    {"UntrustedFileName", fileName}
                }
            };
            // Don't trust any file name, file extension, and file data from the request unless you trust them completely
            // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
            // In short, it is necessary to restrict and verify the upload
            // Here, we just use the temporary folder and a random file name
            var newFileName = Path.GetRandomFileName();

            var result = await UploadFromStreamAsync(fileName, newFileName,FileSizeEnum.Small, stream, options, cancellationToken);

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileUploadResultModel> UploadLargeFileFromStreamAsync(string? fileName, string? contentType, Stream stream,
            CancellationToken cancellationToken = default)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType", contentType},
                    {"UntrustedFileName", fileName}
                }
            };
            // Don't trust any file name, file extension, and file data from the request unless you trust them completely
            // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
            // In short, it is necessary to restrict and verify the upload
            // Here, we just use the temporary folder and a random file name
            var newFileName = Path.GetRandomFileName();

            var result = await UploadFromStreamAsync(fileName, newFileName, FileSizeEnum.Large, stream, options, cancellationToken);

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="filesize"></param>
        /// <param name="source"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileUploadResultModel> UploadFromStreamAsync(
            string? fileName,
            string newFileName,
            FileSizeEnum fileSize,
            Stream source,
            GridFSUploadOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull<string>(fileName, nameof(fileName));
            Ensure.IsNotNull<string>(newFileName, nameof(newFileName));
            Ensure.IsNotNull<Stream>(source, nameof(source));

            var result = new FileUploadResultModel()
            {
                FileName = fileName
            };
            var whiteListResult = ValidateFileWhiteList(fileName);
            if (whiteListResult != ValidationFileEnum.Ok)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = GetValidationMessage(whiteListResult);
                return result;
            }

            options = new GridFSUploadOptions();
            var id = ObjectId.GenerateNewId();
            await using GridFSUploadStream<ObjectId> destination = await ImagesBucket
                .OpenUploadStreamAsync(id, newFileName, options, cancellationToken).ConfigureAwait(false);
            var buffer = new byte[ImagesBucket.Options.ChunkSizeBytes];
            var isFilledFirstBytes = false;
            long lengthOfFile = 0;
            Exception sourceException;


            while (true)
            {
                var bytesRead = 0;
                sourceException = (Exception) null;
                try
                {
                    bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    sourceException = ex;
                }

                if (sourceException == null)
                {
                    if (bytesRead != 0)
                    {
                        if (!isFilledFirstBytes)
                        {
                            var firstBytes = buffer.Take(64).ToArray();
                            var fileExtension = Path.GetExtension(fileName);
                            var signatureResult = ValidateFileSignature(firstBytes, fileExtension);
                            if (signatureResult != ValidationFileEnum.Ok)
                            {
                                await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                                buffer = (byte[]) null;
                                result.ObjectId = destination.Id.ToString();
                                result.IsSuccessful = false;
                                result.ErrorMessage = GetValidationMessage(signatureResult);
                                return result;
                            }

                            isFilledFirstBytes = true;
                        }

                        lengthOfFile += bytesRead;

                        var fileLengthResult = ValidateFileMaxLength(lengthOfFile, fileSize);
                        if (fileLengthResult != ValidationFileEnum.Ok)
                        {
                            try
                            {
                                await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                                await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                            }
                            catch
                            {
                                await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                                await DeleteChunkAsync(destination.Id);
                                //await ImagesBucket.DeleteAsync(destination.Id, cancellationToken);
                            }

                            buffer = (byte[]) null;
                            result.IsSuccessful = false;
                            result.ErrorMessage = GetValidationMessage(fileLengthResult);
                            return result;
                        }

                        await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                        buffer = (byte[])null;

                        var fileLengthResult = ValidateFileMinLength(lengthOfFile, fileSize);
                        if (fileLengthResult != ValidationFileEnum.Ok)
                        {
                            await ImagesBucket.DeleteAsync(destination.Id, cancellationToken);
                            result.IsSuccessful = false;
                            result.ErrorMessage = GetValidationMessage(fileLengthResult);
                        }

                        result.ObjectId = destination.Id.ToString();
                        return result;

                    }
                }
                else
                {
                    try
                    {
                        await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                        await DeleteChunkAsync(destination.Id);
                    }

                    break;
                }
            }

            await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
            buffer = (byte[]) null;

            result.IsSuccessful = false;
            result.ErrorMessage = sourceException.Message + "_" + sourceException.InnerException;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteChunkAsync(ObjectId objectId)
        {
            FilterDefinition<BsonDocument> filter = new BsonDocument("files_id", objectId);
            var chunksCollection =
                MongoDatabase.GetCollection<BsonDocument>(ImagesBucket.Options.BucketName + ".chunks");

            await chunksCollection.DeleteManyAsync(filter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FileDownloadModel?> DownloadAsBytesAsync(ObjectId objectId,
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

            var result = new FileDownloadModel()
            {
                ObjectId = objectId,
                FileInfo = fileInfo,
                FileBytes = bytes
            };

            return result;
        }

        public async Task<FileDownloadModel?> DownloadToStreamAsync(ObjectId objectId, Stream destination,
            CancellationToken cancellationToken = default)
        {
            var fileInfo = await GetFileInfo(objectId);
            if (fileInfo == null)
            {
                return null;
            }

            var result = new FileDownloadModel()
            {
                ObjectId = objectId,
                FileInfo = null
            };
            await ImagesBucket.DownloadToStreamAsync(objectId, destination, null, cancellationToken);
            return result;

        }
    }
}