using Microsoft.AspNetCore.Http;

namespace Nitro.Kernel.Models
{
    public class SmsRequestRecord
    {
        public string Number { get; set; }
        public string Message { get; set; }
    }
}
