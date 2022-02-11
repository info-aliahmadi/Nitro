using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileUploadResultModel
    {
        public ObjectId ObjectId { get; set; }
        public string FileName { get; set; } = null!;
    }

}
