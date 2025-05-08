using MailKit.Net.Smtp;
using MimeKit; 
using Microsoft.Extensions.Options;
namespace Pustakalaya.Helpers;

public class EmailService
{
    public readonly EmailSetting _settings;

    public EmailService(IOptions<EmailSetting> settings)
    {
        _settings = settings.Value;
    }
    
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}