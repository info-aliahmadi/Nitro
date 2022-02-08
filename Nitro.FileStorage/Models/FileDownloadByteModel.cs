using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileDownloadByteModel
    {
        public ObjectId ObjectId { get; set; }
        public byte[] FileStream { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }

}
