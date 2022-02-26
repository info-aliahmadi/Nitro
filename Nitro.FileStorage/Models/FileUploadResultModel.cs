using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileUploadResultModel
    {
        public string ObjectId { get; set; }
        public string? FileName { get; set; } = null!;
        public bool IsSuccessful { get; set; } = true;
        public string ErrorMessage { get; set; } = null!;
    }

}
