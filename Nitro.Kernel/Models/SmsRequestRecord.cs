﻿using Microsoft.AspNetCore.Http;

namespace Nitro.Kernel.Models
{
    public class SmsRequestRecord
    {
        public string ToNumber { get; set; }
        public string Message { get; set; }
    }
}
