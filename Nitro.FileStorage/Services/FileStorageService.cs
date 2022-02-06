using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nitro.FileStorage.Models;

namespace Nitro.FileStorage.Services
{
    public class FileStorageService
    {
        private readonly IMongoCollection<FileObject> _filesCollection;
        public GridFSBucket _imagesBucket { get; set; }

        public FileStorageService(
            IOptions<FileStorageDatabaseSetting> fileStorageDatabaseSetting)
        {
            var mongoClient = new MongoClient(
                fileStorageDatabaseSetting.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                fileStorageDatabaseSetting.Value.DatabaseName);

            _filesCollection = mongoDatabase.GetCollection<FileObject>(
                fileStorageDatabaseSetting.Value.FilesCollectionName);

            _imagesBucket = new GridFSBucket(mongoDatabase);
        }

        public async Task<List<FileObject>> GetAsync() =>
            await _filesCollection.Find(_ => true).ToListAsync();

        public async Task<FileObject?> GetAsync(string id) =>
            await _filesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(FileObject newFile) =>
            await _filesCollection.InsertOneAsync(newFile);

        public async Task UpdateAsync(string id, FileObject updatedFile) =>
            await _filesCollection.ReplaceOneAsync(x => x.Id == id, updatedFile);

        public async Task RemoveAsync(string id) =>
            await _filesCollection.DeleteOneAsync(x => x.Id == id);
        public async Task<ObjectId> Upload(string filename, Stream stream)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    {"ContentType","image/jpeg"}
                }
            };
            var id = await _imagesBucket.UploadFromStreamAsync(filename, stream, options);
            return id;
        }
    }
}
