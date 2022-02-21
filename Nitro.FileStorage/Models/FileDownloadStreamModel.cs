using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileDownloadStreamModel
    {
        public ObjectId ObjectId { get; set; }
        public GridFSFileInfo FileInfo { get; set; } = null!;
    }

}
