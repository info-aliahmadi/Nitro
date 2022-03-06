﻿using Microsoft.AspNetCore.Http;

namespace Nitro.Kernel.Models
{
    public class EmailRequestRecord
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
