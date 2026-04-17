using MimeKit;
using MailKit.Net.Smtp;
using Application.DTOs;
using Application.Interfaces;
using Application.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<Result> SendAsync(EmailDto email)
    {
        try
        {
            _logger.LogInformation(
                "Email фиристода мешавад: Ба [{To}] | Мавзуъ: {Subject}",
                email.To, email.Subject);

            // TODO: MailKit интеграция
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("App", _config["EmailSettings:Email"]));
            message.To.Add(new MailboxAddress("", email.To));
            message.Subject = email.Subject;
            message.Body = new TextPart("html") { Text = email.Body };
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                587,
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            await Task.Delay(10);
            _logger.LogInformation("Email ба [{To}] бомуваффакият фиристода шуд", email.To);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми фиристодани email ба [{To}]", email.To);
            return Result.Failure("Email фиристода нашуд.");
        }
    }

    public Task<Result> SendWelcomeEmailAsync(string toEmail, string userName)
        => SendAsync(new EmailDto
        {
            To = toEmail,
            Subject = "Хуш омадед!",
            Body = $"<h2>Салом, {userName}!</h2><p>Шумо бомуваффакият кайд шудед.</p>"
        });

    public Task<Result> SendPasswordChangedEmailAsync(string toEmail, string userName)
        => SendAsync(new EmailDto
        {
            To = toEmail,
            Subject = "Парол иваз шуд",
            Body = $"<p>Салом, {userName}! Пароли шумо иваз шуд.</p>"
        });
}
