
using Nitro.Kernel.Models;

namespace Nitro.Kernel.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailRequestRecord requestRecord);
    }
}
