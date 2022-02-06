using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nitro.FileStorage.Models
{
    public class FileObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string FileType { get; set; } = null!;

        public DateTime? UploadDateTime { get; set; }

        public long Length { get; set; }

        public string UserId { get; set; } = null!;

    }
}
