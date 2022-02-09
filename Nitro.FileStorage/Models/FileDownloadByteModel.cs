using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileDownloadByteModel
    {
        public ObjectId ObjectId { get; set; }
        public byte[] FileBytes { get; set; } = null!;
        public GridFSFileInfo FileInfo { get; set; } = null!;
    }

}
