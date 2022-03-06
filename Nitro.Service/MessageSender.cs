using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Nitro.Core.Interfaces;
using Nitro.Core.Interfaces.Settings;
using Nitro.Kernel.Interfaces;
using Nitro.Kernel.Models;

namespace Nitro.Service
{
    public class MessageSender : IEmailSender, ISmsSender
    {
        private readonly ISmtpSetting _smtpSetting;
        public MessageSender(ISmtpSetting smtpSetting)
        {
            _smtpSetting = smtpSetting;
        }
        public async Task SendEmailAsync(EmailRequestRecord requestRecord)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_smtpSetting.From);
            email.To.Add(MailboxAddress.Parse(requestRecord.ToEmail));
            email.Subject = requestRecord.Subject;
            var builder = new BodyBuilder();
            if (requestRecord.Attachments.Any())
            {
                foreach (var file in requestRecord.Attachments.Where(file => file.Length > 0))
                {
                    byte[] fileBytes;
                    await using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }
                    builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                }
            }
            builder.HtmlBody = requestRecord.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSetting.SmtpServer, _smtpSetting.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpSetting.From, _smtpSetting.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

        }

        public Task SendSmsAsync(SmsRequestRecord requestRecord)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
