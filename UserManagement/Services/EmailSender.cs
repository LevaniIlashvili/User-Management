using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagement.Models;

namespace UserManagement.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(MailboxAddress.Parse(_settings.From));
        mimeMessage.To.Add(MailboxAddress.Parse(email));
        mimeMessage.Subject = subject;

        mimeMessage.Body = new TextPart("html")
        {
            Text = message
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            _settings.Username,
            _settings.Password
        );

        await client.SendAsync(mimeMessage);
        await client.DisconnectAsync(true);
    }
}
