﻿


namespace Nitro.Core.Interfaces.Settings
{
    public class SmsSetting : ISmsSetting
    {
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
