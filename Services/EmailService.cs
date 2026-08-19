using System.Net.Mail;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Task4.UserManagement.Services;

public class EmailService
{
    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    private readonly IConfiguration _config;

    public async Task SendEmailAsync(string email, string confirmationLink)
    {
        var host = _config["MailtrapSettings:Host"];
        var port = int.Parse(_config["MailtrapSettings:Port"]);
        var username = _config["MailtrapSettings:Username"];
        var password = _config["MailtrapSettings:Password"];
        
        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress("No Reply", _config["MailtrapSettings:SenderEmail"]));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Confirm Email";

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $"Please confirm your account by clicking <a href=\"{confirmationLink}\">here</a>.";
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}