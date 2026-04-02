using System.Net;
using System.Net.Mail;

namespace BookingService.Infrastructure.Notifications
{
    // Concrete service for sending email notifications
    public class EmailNotificationService : IEmailNotification
    {
        // Consider injecting configuration for SMTP server, port, username, password, etc.
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;

        public EmailNotificationService(string smtpHost, int smtpPort, string smtpUser, string smtpPass, string fromEmail)
        {
            _smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true // Use SSL/TLS
            };
            _fromEmail = fromEmail ?? throw new ArgumentNullException(nameof(fromEmail));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentNullException(nameof(toEmail));
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException(nameof(subject));
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentNullException(nameof(body));

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                To = { new MailAddress(toEmail) },
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Set to true if your body contains HTML
            };

            try
            {
                await _smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent successfully to {toEmail}"); // Logging
            }
            catch (Exception ex)
            {
                // Log the exception properly in a real application
                Console.Error.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
                throw; // Re-throw if you want the caller to handle it
            }
        }

        // Implement other email-specific methods if needed
    }

    // Helper interface (optional, but good for DI)
    public interface IEmailNotification
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
