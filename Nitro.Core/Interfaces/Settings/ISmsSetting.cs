

namespace Nitro.Core.Interfaces.Settings
{
    public interface ISmsSetting
    {
        string From { get; set; }
        string SmtpServer { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
    }
}
