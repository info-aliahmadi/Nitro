
using Nitro.Kernel.Models;

namespace Nitro.Kernel.Interfaces
{
    public interface ISmsSender
    {
        Task SendSmsAsync(SmsRequestRecord requestRecord);
    }
}
