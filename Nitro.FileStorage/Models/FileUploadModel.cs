using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Nitro.FileStorage.Models
{
    public class FileUploadModel
    {
        [BindProperty]
        public BufferedFileUpload FileUpload { get; set; }
    }
    public class BufferedFileUpload
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}
