using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileDownloadStreamModel
    {
        public ObjectId ObjectId { get; set; }
        public MemoryStream FileStream { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }

}
