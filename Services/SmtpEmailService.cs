using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string recipientName,
        string resetUrl)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning("Email:SmtpHost is empty, skip sending reset password email.");
            return;
        }

        var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var configuredPort) ? configuredPort : 587;
        var senderEmail = _configuration["Email:SenderEmail"] ?? "noreply@ams.local";
        var senderName = _configuration["Email:SenderName"] ?? "Apartment Management System";
        var smtpUsername = _configuration["Email:Username"];
        var smtpPassword = _configuration["Email:Password"];
        var enableSsl = bool.TryParse(_configuration["Email:EnableSsl"], out var configuredSsl) && configuredSsl;

        using var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Reset your password",
            Body = $"Hello {recipientName},\n\nUse this link to reset your password:\n{resetUrl}\n\nIf you did not request this, you can ignore this email.",
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(toEmail));

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(smtpUsername))
        {
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        }

        await smtpClient.SendMailAsync(message);
    }
}
