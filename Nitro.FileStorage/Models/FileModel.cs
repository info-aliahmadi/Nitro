using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nitro.FileStorage.Models
{
    public class FileModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string UploadDateTime { get; set; } = null!;

        public long Length { get; set; }

        public string UserId { get; set; } = null!;

    }
}
