namespace Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string recipientName, string resetUrl);
}
